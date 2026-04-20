using TourGuideApp.Models;

namespace TourGuideApp;

public partial class PoiDetailPage : ContentPage
{
    private CancellationTokenSource _cts;
    private bool _isNavigating = false;
    private bool _isSpeaking = false;
    private int _speechId = 0;

    public PoiDetailPage(POI selectedPoi)
    {
        InitializeComponent();
        BindingContext = selectedPoi;
        DichGiaoDien();
    }

    private async void DichGiaoDien()
    {
        var poi = BindingContext as POI;
        double dist = poi != null ? Math.Round(poi.DistanceToUser, 2) : 0;
        string lang = App.CurrentLanguageCode ?? "vi";

        btnBack.Text = Services.AppTranslator.Get(lang, "Back");
        lblHome.Text = Services.AppTranslator.Get(lang, "Home");
        lblQr.Text = Services.AppTranslator.Get(lang, "Qr");

        // Dịch nút Danh sách
        lblList.Text = Services.AppTranslator.Get(lang, "List") ?? "Danh sách";

        lblDistance.Text = string.Format(Services.AppTranslator.Get(lang, "Dist"), dist);

        CapNhatNutNghe(false);

        if (poi != null)
        {
            var apiService = new Services.ApiService();
            await apiService.IncreaseViewAsync(poi.Id);
        }
    }

    private void CapNhatNutNghe(bool isSpeaking)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (isSpeaking)
            {
                btnSpeak.Text = "🛑 Stop";
                btnSpeak.BackgroundColor = Color.FromArgb("#DC2626");
            }
            else
            {
                string lang = App.CurrentLanguageCode ?? "vi";
                btnSpeak.Text = Services.AppTranslator.Get(lang, "Speak");
                btnSpeak.BackgroundColor = Color.FromArgb("#2563EB");
            }
        });
    }

    private void OnBackClicked(object sender, EventArgs e) { ExecuteBack(); }

    // Bấm Trang chủ -> Về thẳng MapPage
    private async void OnFooterHomeTapped(object sender, TappedEventArgs e)
    {
        if (_isNavigating) return;
        _isNavigating = true;

        // 1. Lấy danh sách các trang đang mở (Stack)
        var stack = Navigation.NavigationStack.ToList();

        // 2. Tìm xem trang MapPage nằm ở đâu trong chồng đĩa
        var mapPage = stack.FirstOrDefault(p => p is MapPage);

        if (mapPage != null)
        {
            // 3. VÒNG LẶP THẦN THÁNH: Xóa tất cả các trang nằm giữa trang hiện tại và MapPage
            // Duyệt từ trang áp chót ngược về trước
            for (int i = stack.Count - 2; i >= 0; i--)
            {
                var page = stack[i];
                if (page is MapPage) break; // Gặp MapPage thì dừng lại không xóa nữa

                Navigation.RemovePage(page); // Xóa trang rác ở giữa
            }

            // 4. Cuối cùng chỉ cần Pop một cái là nó rơi đúng về MapPage
            await Navigation.PopAsync();
        }
        else
        {
            // Nếu lỡ không tìm thấy MapPage (hiếm gặp) thì mới về gốc
            await Navigation.PopToRootAsync();
        }

        _isNavigating = false;
    }

    // 👉 THÊM MỚI: Bấm Danh sách -> Đẩy trang Danh sách lên hoặc Quay lại
    private async void OnFooterListTapped(object sender, TappedEventArgs e)
    {
        if (_isNavigating) return;
        _isNavigating = true;
        _cts?.Cancel();

        // Vì ta không có dữ liệu List<POI> ở đây, nên ta sẽ đóng trang Chi tiết lại
        // Nó sẽ tự động lùi về trang Danh sách (nếu trước đó khách từ trang Danh sách bấm vào)
        await Navigation.PopAsync();

        _isNavigating = false;
    }

    private async void OnFooterQrTapped(object sender, TappedEventArgs e)
    {
        if (_isNavigating) return;
        _isNavigating = true;
        _cts?.Cancel();
        await Navigation.PushAsync(new QRScannerPage());
        _isNavigating = false;
    }

    private async void ExecuteBack()
    {
        if (_isNavigating) return;
        _isNavigating = true;
        _cts?.Cancel();
        await Navigation.PopAsync();
        _isNavigating = false;
    }

    

    // 👉 THÊM MỚI: Chức năng Share (Chia sẻ) cực xịn
    private async void OnShareClicked(object sender, EventArgs e)
    {
        var poi = BindingContext as POI;
        if (poi != null)
        {
            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Title = "Chia sẻ địa điểm",
                Text = $"Hãy cùng tôi khám phá {poi.Name} qua ứng dụng TourGuide Quận 1!",
                Uri = "https://your-admin-website.com" // Đổi thành link tải app thực tế của Anh sau này
            });
        }
    }

    private async void OnSpeakClicked(object sender, EventArgs e)
    {
        if (btnSpeak.Text.Contains("Stop"))
        {
            _cts?.Cancel();
            btnSpeak.IsEnabled = false;
            await Task.Delay(500);
            CapNhatNutNghe(false);
            btnSpeak.IsEnabled = true;
            return;
        }

        var poi = BindingContext as POI;
        string textToRead = poi?.FinalDescription ?? poi?.Description;

        if (string.IsNullOrEmpty(textToRead) || textToRead.Contains("Chưa có nội dung"))
        {
            await DisplayAlert("Thông báo", "Chưa có nội dung để đọc.", "OK");
            return;
        }

        _cts = new CancellationTokenSource();
        CapNhatNutNghe(true);

        if (poi != null && poi.Id > 0)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var apiService = Handler?.MauiContext?.Services.GetService<Services.ApiService>();
                    if (apiService != null)
                    {
                        await apiService.IncreaseListenAsync(poi.Id);
                    }
                    else
                    {
                        var fallbackApi = new Services.ApiService();
                        await fallbackApi.IncreaseListenAsync(poi.Id);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi tăng lượt nghe: {ex.Message}");
                }
            });
        }

        try
        {
            var locales = await TextToSpeech.Default.GetLocalesAsync();
            string langCode = App.CurrentLanguageCode ?? "vi";
            Locale selectedLocale = locales.FirstOrDefault(l => l.Language.ToLower().Contains(langCode.ToLower()));

            var tuyChon = new SpeechOptions()
            {
                Locale = selectedLocale,
                Pitch = 1.0f,
                Volume = 1.0f
            };

            await TextToSpeech.Default.SpeakAsync(textToRead, tuyChon, cancelToken: _cts.Token);

            if (_cts != null && !_cts.IsCancellationRequested)
            {
                CapNhatNutNghe(false);
            }
        }
        catch (Exception ex)
        {
            CapNhatNutNghe(false);
            if (!(ex is TaskCanceledException))
            {
                await DisplayAlert("Lỗi Hệ Thống Âm Thanh", $"Android từ chối đọc: {ex.Message}", "OK");
            }
        }
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();

        var poi = BindingContext as POI;
        if (poi != null && poi.Id > 0)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var apiService = Handler?.MauiContext?.Services.GetService<Services.ApiService>();
                    if (apiService != null)
                    {
                        await apiService.IncreaseViewAsync(poi.Id);
                    }
                    else
                    {
                        var fallbackApi = new Services.ApiService();
                        await fallbackApi.IncreaseViewAsync(poi.Id);
                    }
                }
                catch { }
            });
        }
    }
}