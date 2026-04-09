using Mapsui;
using Mapsui.Projections;
using System.Globalization;

namespace TourGuideApp;

public partial class MapPage : ContentPage
{
    List<Models.POI> _danhSachPOI;
    Models.POI _diaDiemGanNhat;
    CancellationTokenSource _cts;

    private bool _isNavigating = false;
    private bool _isCentering = false; // Khóa chống bấm liên tục gây lag

    public MapPage(List<Models.POI> pois)
    {
        InitializeComponent();
        _danhSachPOI = pois;

        // XÓA CÁC NÚT ZOOM MẶC ĐỊNH ĐỂ KHÔNG ĐÈ NÚT THOÁT/VỊ TRÍ
        mapView.Map.Widgets.Clear();
        mapView.Map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());

        foreach (var poi in pois)
        {
            double pLat = Convert.ToDouble(poi.Latitude.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
            double pLon = Convert.ToDouble(poi.Longitude.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);

            var pin = new Mapsui.UI.Maui.Pin(mapView)
            {
                Label = poi.Name,
                Position = new Mapsui.UI.Maui.Position(pLat, pLon),
                Type = Mapsui.UI.Maui.PinType.Pin,
                Color = Colors.Red
            };
            mapView.Pins.Add(pin);
        }

        if (_danhSachPOI != null && _danhSachPOI.Count > 0)
        {
            _diaDiemGanNhat = _danhSachPOI[0];
            lblLocationName.Text = _diaDiemGanNhat.Name;
        }

        // TỰ ĐỘNG BAY VỀ VỊ TRÍ KHI VỪA MỞ TRANG
        _ = CenterToUserLocation();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _isNavigating = false;

        // DỊCH NGÔN NGỮ FOOTER VÀ CÁC NÚT (FIX LỖI TIẾNG VIỆT CỨNG)
        if (App.CurrentLanguage == 0) // VN
        {
            btnAudio.Text = "▶ PHÁT THUYẾT MINH";
            lblStatus.Text = "Địa điểm gần bạn:";
            lblHome.Text = "Trang chủ";
            lblQr.Text = "Quét QR";
            if (_diaDiemGanNhat != null) lblDistance.Text = $"Cách bạn: {Math.Round(_diaDiemGanNhat.DistanceToUser, 2)} km";
        }
        else if (App.CurrentLanguage == 1) // EN
        {
            btnAudio.Text = "▶ PLAY AUDIO";
            lblStatus.Text = "Nearby location:";
            lblHome.Text = "Home";
            lblQr.Text = "Scan QR";
            if (_diaDiemGanNhat != null) lblDistance.Text = $"Distance: {Math.Round(_diaDiemGanNhat.DistanceToUser, 2)} km";
        }
        else if (App.CurrentLanguage == 2) // CN
        {
            btnAudio.Text = "▶ 播放语音";
            lblStatus.Text = "附近地点：";
            lblHome.Text = "首页";
            lblQr.Text = "扫码";
            if (_diaDiemGanNhat != null) lblDistance.Text = $"距离：{Math.Round(_diaDiemGanNhat.DistanceToUser, 2)} 公里";
        }
        else // JP
        {
            btnAudio.Text = "▶ 再生する";
            lblStatus.Text = "近くの場所：";
            lblHome.Text = "ホーム";
            lblQr.Text = "QR読取";
            if (_diaDiemGanNhat != null) lblDistance.Text = $"距離：{Math.Round(_diaDiemGanNhat.DistanceToUser, 2)} km";
        }
    }

    private async void OnPinClicked(object sender, Mapsui.UI.Maui.PinClickedEventArgs e)
    {
        if (_isNavigating) return;
        _isNavigating = true;
            
        var clickedPoi = _danhSachPOI.FirstOrDefault(p => p.Name == e.Pin.Label);
        if (clickedPoi != null)
        {
            if (_cts != null && !_cts.IsCancellationRequested) _cts.Cancel();
            await Navigation.PushAsync(new PoiDetailPage(clickedPoi));
            e.Handled = true;
        }
        else { _isNavigating = false; }
    }

    private async void OnCenterUserLocationClicked(object sender, EventArgs e)
    {
        await CenterToUserLocation();
    }

    // ============================================================
    // HÀM LẤY VỊ TRÍ: ĐÃ FIX KÍCH CỠ MẶC ĐỊNH (ZOOM 15)
    // ============================================================
    private async Task CenterToUserLocation()
    {
        if (_isCentering) return;
        _isCentering = true;

        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (status == PermissionStatus.Granted)
            {
                // Ưu tiên lấy vị trí cũ cho nhanh để bản đồ bay đi ngay
                var location = await Geolocation.Default.GetLastKnownLocationAsync();

                if (location == null)
                {
                    // Nếu không có mới dò tìm với độ chính xác vừa phải để tránh lag
                    var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(3));
                    location = await Geolocation.Default.GetLocationAsync(request);
                }

                if (location != null)
                {
                    var userPos = SphericalMercator.FromLonLat(location.Longitude, location.Latitude);

                    // CHÌA KHÓA FIX KÍCH CỠ: Lấy Resolution tương ứng với Zoom Level 15
                    var targetResolution = mapView.Map.Navigator.Resolutions[15];

                    // Thực hiện bay với Zoom 15 trong 500ms
                    mapView.Map.Navigator.FlyTo(new MPoint(userPos.x, userPos.y), targetResolution, 500);

                    mapView.MyLocationLayer.UpdateMyLocation(new Mapsui.UI.Maui.Position(location.Latitude, location.Longitude));
                    mapView.IsMyLocationButtonVisible = true;
                }
            }
        }
        catch { }
        finally
        {
            _isCentering = false;
        }
    }

    private async void OnPlayAudioClicked(object sender, EventArgs e)
    {
        if (_diaDiemGanNhat == null) return;
        string textToRead = _diaDiemGanNhat.Content?.Description ?? _diaDiemGanNhat.Description;
        if (string.IsNullOrEmpty(textToRead)) return;

        if (_cts != null && !_cts.IsCancellationRequested)
        {
            _cts.Cancel();
            OnAppearing();
            btnAudio.BackgroundColor = Color.FromArgb("#2563EB");
            return;
        }

        _cts = new CancellationTokenSource();
        if (App.CurrentLanguage == 0) btnAudio.Text = "⏹ ĐANG ĐỌC...";
        else if (App.CurrentLanguage == 1) btnAudio.Text = "⏹ PLAYING...";
        else if (App.CurrentLanguage == 2) btnAudio.Text = "⏹ 播放中...";
        else btnAudio.Text = "⏹ 再生中...";

        btnAudio.BackgroundColor = Color.FromArgb("#DC2626");

        try
        {
            var locales = await TextToSpeech.Default.GetLocalesAsync();
            Locale selectedLocale = null;
            if (App.CurrentLanguage == 0) selectedLocale = locales.FirstOrDefault(l => l.Language.ToLower().Contains("vi"));
            else if (App.CurrentLanguage == 1) selectedLocale = locales.FirstOrDefault(l => l.Language.ToLower().Contains("en"));
            else if (App.CurrentLanguage == 2) selectedLocale = locales.FirstOrDefault(l => l.Language.ToLower().Contains("zh"));
            else if (App.CurrentLanguage == 3) selectedLocale = locales.FirstOrDefault(l => l.Language.ToLower().Contains("ja"));

            await TextToSpeech.Default.SpeakAsync(textToRead, new SpeechOptions() { Locale = selectedLocale, Pitch = 1.0f, Volume = 1.0f }, cancelToken: _cts.Token);
            OnAppearing(); btnAudio.BackgroundColor = Color.FromArgb("#2563EB");
        }
        catch { }
    }

    private async void OnScanQrClicked(object sender, TappedEventArgs e)
    {
        if (_cts != null && !_cts.IsCancellationRequested) _cts.Cancel();
        await Navigation.PushAsync(new QRScannerPage(_danhSachPOI));
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (_cts != null && !_cts.IsCancellationRequested) _cts.Cancel();
    }

    private async void OnExitToMainClicked(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }
}