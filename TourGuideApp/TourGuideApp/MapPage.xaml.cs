//using Android.Bluetooth.LE;
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

    // Khai báo biến cho hệ thống Tracking
    private string _deviceId;
    private IDispatcherTimer _pingTimer;

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
    // 1. TỰ ĐỘNG DỊCH & BẬT CẢM BIẾN MẠNG KHI MỞ TRANG
    // ==========================================
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _isNavigating = false;

        // --- HỆ THỐNG TRACKING ONLINE ---
        if (string.IsNullOrEmpty(_deviceId))
        {
            // Tự động tạo một cái tên riêng biệt cho điện thoại này (Vd: iPhone_8A1B)
            _deviceId = DeviceInfo.Current.Name.Replace(" ", "") + "_" + Guid.NewGuid().ToString().Substring(0, 5);
        }
        SetupNetworkTracking(); // Bật đồng hồ nhịp tim

        // --- HỆ THỐNG DỊCH GIAO DIỆN ---
        string lang = App.CurrentLanguageCode;
        btnAudio.Text = Services.AppTranslator.Get(lang, "Speak");
        lblStatus.Text = Services.AppTranslator.Get(lang, "Near");
        lblHome.Text = Services.AppTranslator.Get(lang, "Home");
        lblQr.Text = Services.AppTranslator.Get(lang, "Qr");

        if (_diaDiemGanNhat != null)
        {
            lblDistance.Text = string.Format(Services.AppTranslator.Get(lang, "Dist"), Math.Round(_diaDiemGanNhat.DistanceToUser, 2));
        }
    }

    // ==========================================
    // CÁC HÀM XỬ LÝ NHỊP TIM VÀ ONLINE/OFFLINE
    // ==========================================
    private void SetupNetworkTracking()
    {
        // 1. Kiểm tra ngay lúc mới mở app có mạng không
        UpdateNetworkStatus(Connectivity.Current.NetworkAccess == NetworkAccess.Internet);

        // 2. Lắng nghe lúc khách tắt/bật Wifi
        Connectivity.Current.ConnectivityChanged += Current_ConnectivityChanged;

        // 3. Cài đồng hồ 5 giây gửi tín hiệu báo cáo cho Admin 1 lần
        if (_pingTimer == null)
        {
            _pingTimer = Dispatcher.CreateTimer();
            _pingTimer.Interval = TimeSpan.FromSeconds(5);
            _pingTimer.Tick += async (s, e) =>
            {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    var apiService = Handler?.MauiContext?.Services.GetService<Services.ApiService>() ?? new Services.ApiService();
                    await apiService.PingTrackingAsync(_deviceId);
                }
            };
        }
        _pingTimer.Start();
    }

    private void Current_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
    {
        UpdateNetworkStatus(e.NetworkAccess == NetworkAccess.Internet);
    }

    private void UpdateNetworkStatus(bool isOnline)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            // Nhớ đổi tên ở chỗ kiểm tra null này nữa nhé
            if (frmNetworkStatus != null && dotStatus != null && lblNetStatus != null)
            {
                if (isOnline)
                {
                    frmNetworkStatus.BackgroundColor = Color.FromArgb("#dcfce7");
                    dotStatus.Color = Color.FromArgb("#22c55e");

                    lblNetStatus.Text = "Online";                     // ĐỔI TÊN Ở ĐÂY
                    lblNetStatus.TextColor = Color.FromArgb("#166534"); // ĐỔI TÊN Ở ĐÂY
                }
                else
                {
                    frmNetworkStatus.BackgroundColor = Color.FromArgb("#fee2e2");
                    dotStatus.Color = Color.FromArgb("#ef4444");

                    lblNetStatus.Text = "Offline";                    // ĐỔI TÊN Ở ĐÂY
                    lblNetStatus.TextColor = Color.FromArgb("#991b1b"); // ĐỔI TÊN Ở ĐÂY
                }
            }
        });
    }

    // ==========================================
    // CÁC CHỨC NĂNG BẢN ĐỒ VÀ AUDIO
    // ==========================================
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
                    // 1. CHỈNH TÂM BẢN ĐỒ VÀO CHẤM XANH
                    var userPos = SphericalMercator.FromLonLat(location.Longitude, location.Latitude);
                    var targetPoint = new MPoint(userPos.x, userPos.y);

                    mapView.Map.Navigator.CenterOn(targetPoint);

                    double zoomLevel = 15;
                    double targetResolution = 156543.03390625 / Math.Pow(2, zoomLevel);

                    mapView.Map.Navigator.ZoomTo(targetResolution);

                    mapView.MyLocationLayer.UpdateMyLocation(new Mapsui.UI.Maui.Position(location.Latitude, location.Longitude));
                    mapView.IsMyLocationButtonVisible = true;

                    // ================================================================
                    // 🚀 BƯỚC MỚI: TÍNH KHOẢNG CÁCH VÀ TÌM ĐỊA ĐIỂM GẦN NHẤT THỰC TẾ
                    // ================================================================
                    if (_danhSachPOI != null && _danhSachPOI.Count > 0)
                    {
                        foreach (var poi in _danhSachPOI)
                        {
                            double pLat = Convert.ToDouble(poi.Latitude.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
                            double pLon = Convert.ToDouble(poi.Longitude.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);

                            var poiLocation = new Location(pLat, pLon);

                            // MAUI hỗ trợ sẵn hàm đo khoảng cách đường chim bay cực xịn
                            poi.DistanceToUser = Location.CalculateDistance(location, poiLocation, DistanceUnits.Kilometers);
                        }

                        // Áp dụng thuật toán: Tìm đứa gần nhất, nếu bằng nhau thì đứa nào Độ ưu tiên cao hơn sẽ thắng
                        _diaDiemGanNhat = _danhSachPOI
                            .OrderBy(p => p.DistanceToUser)
                            .ThenByDescending(p => p.PriorityScore)
                            .FirstOrDefault();

                        // Cập nhật lại giao diện ngay lập tức
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            if (_diaDiemGanNhat != null)
                            {
                                lblLocationName.Text = _diaDiemGanNhat.Name;
                                string lang = App.CurrentLanguageCode;
                                lblDistance.Text = string.Format(Services.AppTranslator.Get(lang, "Dist"), Math.Round(_diaDiemGanNhat.DistanceToUser, 2));
                            }
                        });
                        // ================================================================
                        // 🚀 BƯỚC MỚI: TÍNH KHOẢNG CÁCH VÀ TÌM ĐỊA ĐIỂM GẦN NHẤT THỰC TẾ
                        // ================================================================
                        if (_danhSachPOI != null && _danhSachPOI.Count > 0)
                        {
                            foreach (var poi in _danhSachPOI)
                            {
                                try
                                {
                                    double pLat = Convert.ToDouble(poi.Latitude.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
                                    double pLon = Convert.ToDouble(poi.Longitude.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);

                                    var poiLocation = new Location(pLat, pLon);
                                    // MAUI hỗ trợ sẵn hàm đo khoảng cách
                                    poi.DistanceToUser = Location.CalculateDistance(location, poiLocation, DistanceUnits.Kilometers);
                                }
                                catch
                                {
                                    // BỌC THÉP Ở ĐÂY: Nếu Admin nhập sai tọa độ gây lỗi, gán khoảng cách xa vô tận
                                    poi.DistanceToUser = 999999;
                                }
                            }

                            // Áp dụng thuật toán: Tìm đứa gần nhất, nếu bằng nhau thì đứa nào Độ ưu tiên cao hơn sẽ thắng
                            _diaDiemGanNhat = _danhSachPOI
                                .OrderBy(p => p.DistanceToUser)
                                .ThenByDescending(p => p.PriorityScore)
                                .FirstOrDefault();

                            // Cập nhật lại giao diện ngay lập tức
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                if (_diaDiemGanNhat != null)
                                {
                                    lblLocationName.Text = _diaDiemGanNhat.Name;
                                    string lang = App.CurrentLanguageCode;
                                    lblDistance.Text = string.Format(Services.AppTranslator.Get(lang, "Dist"), Math.Round(_diaDiemGanNhat.DistanceToUser, 2));
                                }
                            });
                        }
                    }
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

    private async void OnPlayAudioClicked(object sender, EventArgs e)
    {
        if (_diaDiemGanNhat == null) return;

        string textToRead = _diaDiemGanNhat.FinalDescription;
        if (string.IsNullOrEmpty(textToRead) || textToRead.Contains("Chưa có nội dung")) return;

        if (_cts != null && !_cts.IsCancellationRequested)
        {
            _cts.Cancel();
            OnAppearing();
            btnAudio.BackgroundColor = Color.FromArgb("#2563EB");
            return;
        }

        _cts = new CancellationTokenSource();

        btnAudio.Text = "🛑 Stop";
        btnAudio.BackgroundColor = Color.FromArgb("#DC2626");

        try
        {

            // MAUI hỗ trợ sẵn TextToSpeech, chỉ cần gọi lên là đọc được, cực kỳ đơn giản
            var locales = await TextToSpeech.Default.GetLocalesAsync();
            string langCode = App.CurrentLanguageCode;

            Locale selectedLocale = locales.FirstOrDefault(l => l.Language.ToLower().Contains(langCode));

            await TextToSpeech.Default.SpeakAsync(textToRead, new SpeechOptions() { Locale = selectedLocale, Pitch = 1.0f, Volume = 1.0f }, cancelToken: _cts.Token);
        }
        catch { }
        finally
        {
            OnAppearing();
            btnAudio.BackgroundColor = Color.FromArgb("#2563EB");
        }
    }

    private async void OnScanQrClicked(object sender, TappedEventArgs e)
    {
        if (_cts != null && !_cts.IsCancellationRequested) _cts.Cancel();
        await Navigation.PushAsync(new QRScannerPage());
    }

    // ==========================================
    // DỌN DẸP KHI THOÁT KHỎI TRANG ĐỂ ĐỠ TỐN PIN
    // ==========================================
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (_cts != null && !_cts.IsCancellationRequested) _cts.Cancel();

        // Khách thoát khỏi bản đồ thì tắt máy đo nhịp tim
        _pingTimer?.Stop();
        Connectivity.Current.ConnectivityChanged -= Current_ConnectivityChanged;
    }

    private async void OnExitToMainClicked(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }
}