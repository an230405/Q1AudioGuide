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

    private void DichGiaoDien()
    {
        var poi = BindingContext as POI;
        double dist = poi != null ? Math.Round(poi.DistanceToUser, 2) : 0;
        string lang = App.CurrentLanguageCode ?? "vi";

        btnBack.Text = Services.AppTranslator.Get(lang, "Back");
        lblHome.Text = Services.AppTranslator.Get(lang, "Home");
        lblQr.Text = Services.AppTranslator.Get(lang, "Qr");
        lblDistance.Text = string.Format(Services.AppTranslator.Get(lang, "Dist"), dist);

            CapNhatNutNghe(false);
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
            btnSpeak.IsEnabled = false; // Khóa nút nửa giây cho loa tắt hẳn
            await Task.Delay(500);
            CapNhatNutNghe(false);      // Trả về nút Xanh
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
        CapNhatNutNghe(true); // Sang màu Đỏ ngay lập tức!

        try
        {
            // Bỏ đi mọi cài đặt rườm rà, để hệ điều hành tự lo giọng đọc
            await TextToSpeech.Default.SpeakAsync(textToRead, cancelToken: _cts.Token);

            // NẾU ĐỌC XONG TRỌN VẸN (Không bị Cancel) -> Mới tự động trả về nút Xanh
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                CapNhatNutNghe(false);
            }
        }
        catch (Exception ex)
        {
            // NẾU LỖI NGẦM TỪ ANDROID -> BẮT NÓ HIỆN RA ĐỂ MÌNH THẤY!
            CapNhatNutNghe(false);
            if (!(ex is TaskCanceledException)) // Bỏ qua lỗi hủy khi bấm Stop
            {
                await DisplayAlert("Lỗi Hệ Thống Âm Thanh", $"Android từ chối đọc: {ex.Message}", "OK");
            }
        }
    }
}