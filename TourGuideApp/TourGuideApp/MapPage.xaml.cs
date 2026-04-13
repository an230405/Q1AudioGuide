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
    private bool _isCentering = false;

    public MapPage(List<Models.POI> pois)
    {
        InitializeComponent();
        _danhSachPOI = pois;

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

        _ = CenterToUserLocation();
    }

    // ==========================================
    // 1. TỰ ĐỘNG DỊCH GIAO DIỆN KHI MỞ TRANG
    // ==========================================
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _isNavigating = false;

        string lang = App.CurrentLanguageCode; // Lấy mã "th", "vi", "en"...

        // GỌI TỪ ĐIỂN RA XÀI - XÓA SẠCH IF-ELSE CŨ
        btnAudio.Text = Services.AppTranslator.Get(lang, "Speak");
        lblStatus.Text = Services.AppTranslator.Get(lang, "Near");
        lblHome.Text = Services.AppTranslator.Get(lang, "Home");
        lblQr.Text = Services.AppTranslator.Get(lang, "Qr");

        if (_diaDiemGanNhat != null)
        {
            lblDistance.Text = string.Format(Services.AppTranslator.Get(lang, "Dist"), Math.Round(_diaDiemGanNhat.DistanceToUser, 2));
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
                var location = await Geolocation.Default.GetLastKnownLocationAsync();

                if (location == null)
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(3));
                    location = await Geolocation.Default.GetLocationAsync(request);
                }

                if (location != null)
                {
                    var userPos = SphericalMercator.FromLonLat(location.Longitude, location.Latitude);
                    var targetPoint = new MPoint(userPos.x, userPos.y);

                    mapView.Map.Navigator.CenterOn(targetPoint);

                    double zoomLevel = 15;
                    double targetResolution = 156543.03390625 / Math.Pow(2, zoomLevel);

                    mapView.Map.Navigator.ZoomTo(targetResolution);

                    mapView.MyLocationLayer.UpdateMyLocation(new Mapsui.UI.Maui.Position(location.Latitude, location.Longitude));
                    mapView.IsMyLocationButtonVisible = true;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi Zoom: {ex.Message}");
        }
        finally
        {
            _isCentering = false;
        }
    }

    // ==========================================
    // 2. TỐI ƯU HÓA NÚT PHÁT AUDIO (ĐỌC TIẾNG THÁI)
    // ==========================================
    private async void OnPlayAudioClicked(object sender, EventArgs e)
    {
        if (_diaDiemGanNhat == null) return;

        // Dùng FinalDescription để luôn lấy được text chính xác
        string textToRead = _diaDiemGanNhat.FinalDescription;
        if (string.IsNullOrEmpty(textToRead) || textToRead.Contains("Chưa có nội dung")) return;

        if (_cts != null && !_cts.IsCancellationRequested)
        {
            _cts.Cancel();
            OnAppearing(); // Gọi lại hàm dịch để reset chữ nút bấm
            btnAudio.BackgroundColor = Color.FromArgb("#2563EB");
            return;
        }

        _cts = new CancellationTokenSource();

        // Khi đang phát Audio thì hiện chung chữ Stop cho mọi ngôn ngữ
        btnAudio.Text = "🛑 Stop";
        btnAudio.BackgroundColor = Color.FromArgb("#DC2626");

        try
        {
            var locales = await TextToSpeech.Default.GetLocalesAsync();
            string langCode = App.CurrentLanguageCode; // Tự bắt mã ngôn ngữ (th, vi, en)

            // Tìm đúng giọng đọc của quốc gia đó
            Locale selectedLocale = locales.FirstOrDefault(l => l.Language.ToLower().Contains(langCode));

            await TextToSpeech.Default.SpeakAsync(textToRead, new SpeechOptions() { Locale = selectedLocale, Pitch = 1.0f, Volume = 1.0f }, cancelToken: _cts.Token);
        }
        catch { }
        finally
        {
            OnAppearing(); // Đọc xong thì reset chữ lại
            btnAudio.BackgroundColor = Color.FromArgb("#2563EB");
        }
    }

    private async void OnScanQrClicked(object sender, TappedEventArgs e)
    {
        if (_cts != null && !_cts.IsCancellationRequested) _cts.Cancel();
        await Navigation.PushAsync(new QRScannerPage());
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