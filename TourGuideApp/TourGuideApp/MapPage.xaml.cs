using Mapsui;
using Mapsui.Projections;
using System.Globalization;
using Microsoft.Maui.Devices.Sensors;

namespace TourGuideApp;

public partial class MapPage : ContentPage
{
    List<Models.POI> _danhSachPOI;
    Models.POI _diaDiemGanNhat;
    CancellationTokenSource _cts;

    private bool _isNavigating = false;
    private bool _isCentering = false;

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

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _isNavigating = false;

        if (string.IsNullOrEmpty(_deviceId))
        {
            _deviceId = DeviceInfo.Current.Name.Replace(" ", "") + "_" + Guid.NewGuid().ToString().Substring(0, 5);
        }
        SetupNetworkTracking();

        string lang = App.CurrentLanguageCode;
        btnAudio.Text = Services.AppTranslator.Get(lang, "Speak");
        lblStatus.Text = Services.AppTranslator.Get(lang, "Near");
        lblHome.Text = Services.AppTranslator.Get(lang, "Home");
        lblQr.Text = Services.AppTranslator.Get(lang, "Qr");

        // Dịch tên nút Danh sách nếu có trong từ điển, không thì để mặc định tiếng Việt
        lblList.Text = Services.AppTranslator.Get(lang, "List") ?? "Danh sách";

        if (_diaDiemGanNhat != null)
        {
            lblDistance.Text = string.Format(Services.AppTranslator.Get(lang, "Dist"), Math.Round(_diaDiemGanNhat.DistanceToUser, 2));
        }

        await StartLocationTracking();
    }

    private void SetupNetworkTracking()
    {
        UpdateNetworkStatus(Connectivity.Current.NetworkAccess == NetworkAccess.Internet);

        Connectivity.Current.ConnectivityChanged += Current_ConnectivityChanged;

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
            if (frmNetworkStatus != null && dotStatus != null && lblNetStatus != null)
            {
                if (isOnline)
                {
                    frmNetworkStatus.BackgroundColor = Color.FromArgb("#dcfce7");
                    dotStatus.Color = Color.FromArgb("#22c55e");
                    lblNetStatus.Text = "Online";
                    lblNetStatus.TextColor = Color.FromArgb("#166534");
                }
                else
                {
                    frmNetworkStatus.BackgroundColor = Color.FromArgb("#fee2e2");
                    dotStatus.Color = Color.FromArgb("#ef4444");
                    lblNetStatus.Text = "Offline";
                    lblNetStatus.TextColor = Color.FromArgb("#991b1b");
                }
            }
        });
    }

    private async Task StartLocationTracking()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

        if (status != PermissionStatus.Granted) return;

        var request = new GeolocationListeningRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(3));
        Geolocation.Default.LocationChanged += OnLocationChanged;
        await Geolocation.Default.StartListeningForegroundAsync(request);
    }

    private void StopLocationTracking()
    {
        Geolocation.Default.LocationChanged -= OnLocationChanged;
        Geolocation.Default.StopListeningForeground();
    }

    private void OnLocationChanged(object sender, GeolocationLocationChangedEventArgs e)
    {
        ProcessGeofence(e.Location);
    }

    private async void ProcessGeofence(Location currentLocation)
    {
        if (_danhSachPOI == null || !_danhSachPOI.Any()) return;

        foreach (var poi in _danhSachPOI)
        {
            double pLat = Convert.ToDouble(poi.Latitude.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
            double pLon = Convert.ToDouble(poi.Longitude.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);

            double distance = Location.CalculateDistance(
                currentLocation,
                new Location(pLat, pLon),
                DistanceUnits.Kilometers) * 1000;
            // Sử dụng bán kính từ API nếu có, nếu không thì mặc định 30m
            double radius = poi.Radius > 0 ? poi.Radius : 30;

            if (distance <= radius && !poi.IsAutoPlayed)
            {
                poi.IsAutoPlayed = true;
                await PlayAutoAudio(poi);
                break;
            }
            else if (distance > radius + 30)
            {
                poi.IsAutoPlayed = false;
            }
        }
    }

    private async Task PlayAutoAudio(Models.POI poi)
    {
        string textToRead = poi.FinalDescription ?? poi.Description;
        if (string.IsNullOrEmpty(textToRead) || textToRead.Contains("Chưa có nội dung")) return;

        if (_cts != null && !_cts.IsCancellationRequested)
        {
            _cts.Cancel();
        }

        _cts = new CancellationTokenSource();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            btnAudio.Text = "🛑 Stop";
            btnAudio.BackgroundColor = Color.FromArgb("#DC2626");
        });

        try
        {
            var locales = await TextToSpeech.Default.GetLocalesAsync();
            string langCode = App.CurrentLanguageCode ?? "vi";
            var selectedLocale = locales.FirstOrDefault(l => l.Language.ToLower().Contains(langCode.ToLower()));

            var options = new SpeechOptions
            {
                Locale = selectedLocale,
                Pitch = 1.0f,
                Volume = 1.0f
            };

            await TextToSpeech.Default.SpeakAsync(textToRead, options, cancelToken: _cts.Token);
        }
        catch { }
        finally
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                OnAppearing();
                btnAudio.BackgroundColor = Color.FromArgb("#2563EB");
            });
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

                    if (_danhSachPOI != null && _danhSachPOI.Count > 0)
                    {
                        foreach (var poi in _danhSachPOI)
                        {
                            try
                            {
                                double pLat = Convert.ToDouble(poi.Latitude.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
                                double pLon = Convert.ToDouble(poi.Longitude.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);

                                var poiLocation = new Location(pLat, pLon);
                                poi.DistanceToUser = Location.CalculateDistance(location, poiLocation, DistanceUnits.Kilometers);
                            }
                            catch
                            {
                                poi.DistanceToUser = 999999;
                            }
                        }

                        _diaDiemGanNhat = _danhSachPOI
                            .OrderBy(p => p.DistanceToUser)
                            .ThenByDescending(p => p.PriorityScore)
                            .FirstOrDefault();

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

        string textToRead = _diaDiemGanNhat.FinalDescription ?? _diaDiemGanNhat.Description;
        if (string.IsNullOrEmpty(textToRead) || textToRead.Contains("Chưa có nội dung"))
        {
            await DisplayAlert("Thông báo", "Chưa có nội dung thuyết minh.", "OK");
            return;
        }

        // 1. XỬ LÝ KHI BẤM NÚT STOP TRONG LÚC ĐANG PHÁT
        if (_cts != null && !_cts.IsCancellationRequested)
        {
            _cts.Cancel();
            // Không cần làm gì thêm ở đây, vì khi Cancel thì code sẽ tự nhảy xuống khối finally bên dưới
            return;
        }

        // 2. CHUẨN BỊ BẮT ĐẦU PHÁT
        _cts = new CancellationTokenSource();
        string lang = App.CurrentLanguageCode;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            btnAudio.Text = "🛑 Stop";
            btnAudio.BackgroundColor = Color.FromArgb("#DC2626");
        });

        try
        {
            var locales = await TextToSpeech.Default.GetLocalesAsync();
            var selectedLocale = locales.FirstOrDefault(l => l.Language != null && l.Language.ToLower().Contains(lang.ToLower()));

            var options = new SpeechOptions { Locale = selectedLocale, Pitch = 1.0f, Volume = 1.0f };

            // Lệnh này sẽ treo ở đây cho đến khi đọc xong hoặc bị Cancel
            await TextToSpeech.Default.SpeakAsync(textToRead, options, cancelToken: _cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Khách bấm Stop, không cần xử lý gì đặc biệt
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi TTS: {ex.Message}");
        }
        finally
        {
            // 3. QUAN TRỌNG NHẤT: RESET NÚT BẤM SAU KHI KẾT THÚC (DÙ LÀ ĐỌC HẾT HAY BẤM DỪNG)
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Lấy lại chữ "Nghe" theo đúng ngôn ngữ đang chọn
                btnAudio.Text = Services.AppTranslator.Get(lang, "Speak");
                btnAudio.BackgroundColor = Color.FromArgb("#2563EB");
            });

            _cts?.Dispose();
            _cts = null;
        }
    }

    // 👉 ĐÃ SỬA: Thêm hàm xử lý khi bấm nút Danh sách
    private async void OnListClicked(object sender, TappedEventArgs e)
    {
        // Tắt Audio nếu đang phát trước khi chuyển trang
        if (_cts != null && !_cts.IsCancellationRequested)
        {
            _cts.Cancel();
        }
        await Navigation.PushAsync(new PoiListPage(_danhSachPOI));
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

        _pingTimer?.Stop();
        Connectivity.Current.ConnectivityChanged -= Current_ConnectivityChanged;

        StopLocationTracking();
    }

    private async void OnExitToMainClicked(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }
}