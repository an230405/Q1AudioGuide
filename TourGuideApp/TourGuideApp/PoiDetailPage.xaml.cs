using TourGuideApp.Models;

namespace TourGuideApp;

public partial class PoiDetailPage : ContentPage
{
    private CancellationTokenSource _cts;
    private bool _isNavigating = false;
    private bool _isSpeaking = false;

    // VŨ KHÍ TỐI THƯỢNG: Đánh dấu số thứ tự để chống chớp giật nút
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
        lblDistance.Text = string.Format(Services.AppTranslator.Get(lang, "Dist"), dist);

        CapNhatNutNghe(false);

        // 👉 THÊM MỚI: Bắn pháo sáng báo cáo Lượt Xem
        if (poi != null)
        {
            var apiService = new Services.ApiService();
            await apiService.IncreaseViewAsync(poi.Id);
        }
    }

    // Tách riêng hàm đổi màu nút ra để quản lý độc quyền trên luồng chính
    private void CapNhatNutNghe(bool isSpeaking)
    {
        // Ép chạy trên luồng giao diện chính để chống giật
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
    private void OnFooterHomeTapped(object sender, TappedEventArgs e) { ExecuteBack(); }

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

    private async void OnSpeakClicked(object sender, EventArgs e)
    {
        // 1. NẾU ĐANG LÀ NÚT STOP -> BẤM ĐỂ DỪNG
        if (btnSpeak.Text.Contains("Stop"))
        {
            _cts?.Cancel();
            btnSpeak.IsEnabled = false;
            await Task.Delay(500);
            CapNhatNutNghe(false);
            btnSpeak.IsEnabled = true;
            return;
        }

        // 2. NẾU ĐANG LÀ NÚT NGHE -> BẮT ĐẦU ĐỌC
        var poi = BindingContext as POI;
        string textToRead = poi?.FinalDescription ?? poi?.Description;

        if (string.IsNullOrEmpty(textToRead) || textToRead.Contains("Chưa có nội dung"))
        {
            await DisplayAlert("Thông báo", "Chưa có nội dung để đọc.", "OK");
            return;
        }

        _cts = new CancellationTokenSource();
        CapNhatNutNghe(true);

        // 🚀 BƯỚC MỚI: BẮN TÍN HIỆU "THU TIỀN" VỀ SERVER (SỬ DỤNG MAUI DI)
        if (poi != null && poi.Id > 0)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    // Gọi API thông qua Handler của hệ thống MAUI (Cách xịn nhất)
                    var apiService = Handler?.MauiContext?.Services.GetService<Services.ApiService>();
                    if (apiService != null)
                    {
                        await apiService.IncreaseListenAsync(poi.Id);
                    }
                    else
                    {
                        // Dự phòng nếu không lấy được từ kho
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

        // 3. THỰC HIỆN ĐỌC ÂM THANH
        try
        {
            // 👉👉👉 ĐOẠN THÊM MỚI: TÌM VÀ ÉP GIỌNG ĐỌC CHO ĐIỆN THOẠI 👉👉👉
            // Bước A: Lấy danh sách tất cả các giọng đọc mà điện thoại đang có
            var locales = await TextToSpeech.Default.GetLocalesAsync();

            // Bước B: Lấy mã ngôn ngữ hiện tại khách đang chọn trên App (vd: "en", "ko")
            string langCode = App.CurrentLanguageCode ?? "vi"; // Mặc định là vi nếu bị null

            // Bước C: Tìm cái giọng khớp với mã ngôn ngữ đó
            Locale selectedLocale = locales.FirstOrDefault(l => l.Language.ToLower().Contains(langCode.ToLower()));

            // Bước D: Đưa cái giọng vừa tìm được vào SpeechOptions và bắt điện thoại đọc
            var tuyChon = new SpeechOptions()
            {
                Locale = selectedLocale,
                Pitch = 1.0f,
                Volume = 1.0f
            };

            await TextToSpeech.Default.SpeakAsync(textToRead, tuyChon, cancelToken: _cts.Token);
            // 👈👈👈 KẾT THÚC ĐOẠN THÊM MỚI 👈👈👈

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
            // Chạy ngầm tăng View ngay khi trang vừa mở lên
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
                catch { } // Không cần in lỗi
            });
        }
    }
}