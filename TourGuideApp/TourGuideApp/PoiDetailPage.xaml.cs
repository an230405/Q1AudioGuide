using TourGuideApp.Models;

namespace TourGuideApp;

public partial class PoiDetailPage : ContentPage
{
    private CancellationTokenSource _cts;
    private bool _isNavigating = false;

    public PoiDetailPage(POI selectedPoi)
    {
        InitializeComponent();
        BindingContext = selectedPoi;

        if (selectedPoi != null)
        {
            imgPoi.Source = selectedPoi.ImageUrl;
            lblName.Text = selectedPoi.Name;

            // FIX LỖI THIẾU TEXT DỰ PHÒNG
            if (selectedPoi.Content != null && !string.IsNullOrEmpty(selectedPoi.Content.Description))
                lblDescription.Text = selectedPoi.Content.Description;
            else
                lblDescription.Text = selectedPoi.Description ?? "(Chưa có nội dung)";
        }

        DichGiaoDien();
    }

    private void DichGiaoDien()
    {
        var poi = BindingContext as POI;
        double dist = poi != null ? Math.Round(poi.DistanceToUser, 2) : 0;

        if (App.CurrentLanguage == 0) // VN
        {
            btnBack.Text = "❮ Quay lại";
            btnSpeak.Text = "🔊 Nghe thuyết minh";
            lblHome.Text = "Trang chủ";
            lblQr.Text = "Quét QR";
            lblDistance.Text = $"📍 Cách bạn: {dist} km";
        }
        else if (App.CurrentLanguage == 1) // EN
        {
            btnBack.Text = "❮ Back";
            btnSpeak.Text = "🔊 Play Audio";
            lblHome.Text = "Home";
            lblQr.Text = "Scan QR";
            lblDistance.Text = $"📍 Distance: {dist} km";
        }
        else if (App.CurrentLanguage == 2) // CN
        {
            btnBack.Text = "❮ 返回";
            btnSpeak.Text = "🔊 播放语音";
            lblHome.Text = "首页";
            lblQr.Text = "扫码";
            lblDistance.Text = $"📍 距离：{dist} 公里";
        }
        else // JP
        {
            btnBack.Text = "❮ 戻る";
            btnSpeak.Text = "🔊 再生する";
            lblHome.Text = "ホーム";
            lblQr.Text = "QR読取";
            lblDistance.Text = $"📍 距離：{dist} km";
        }
    }

    private void OnBackClicked(object sender, EventArgs e) { ExecuteBack(); }
    private void OnFooterHomeTapped(object sender, TappedEventArgs e) { ExecuteBack(); }

    private async void ExecuteBack()
    {
        if (_isNavigating) return;
        _isNavigating = true;
        if (_cts != null && !_cts.IsCancellationRequested) _cts.Cancel();
        await Navigation.PopAsync();
        _isNavigating = false;
    }

    private async void OnSpeakClicked(object sender, EventArgs e)
    {
        var poi = BindingContext as POI;
        string textToRead = poi?.Content?.Description ?? poi?.Description;
        if (string.IsNullOrEmpty(textToRead)) return;

        if (_cts?.IsCancellationRequested == false)
        {
            _cts.Cancel();
            DichGiaoDien();
            btnSpeak.BackgroundColor = Color.FromArgb("#2563EB");
            return;
        }

        _cts = new CancellationTokenSource();
        if (App.CurrentLanguage == 0) btnSpeak.Text = "🛑 Dừng"; else if (App.CurrentLanguage == 1) btnSpeak.Text = "🛑 Stop"; else if (App.CurrentLanguage == 2) btnSpeak.Text = "🛑 停止"; else btnSpeak.Text = "🛑 停止";
        btnSpeak.BackgroundColor = Color.FromArgb("#DC2626");

        try
        {
            var locales = await TextToSpeech.Default.GetLocalesAsync();
            Locale selectedLocale = null;
            if (App.CurrentLanguage == 0) selectedLocale = locales.FirstOrDefault(l => l.Language.ToLower().Contains("vi"));
            else if (App.CurrentLanguage == 1) selectedLocale = locales.FirstOrDefault(l => l.Language.ToLower().Contains("en"));
            else if (App.CurrentLanguage == 2) selectedLocale = locales.FirstOrDefault(l => l.Language.ToLower().Contains("zh"));
            else if (App.CurrentLanguage == 3) selectedLocale = locales.FirstOrDefault(l => l.Language.ToLower().Contains("ja"));

            await TextToSpeech.Default.SpeakAsync(textToRead, new SpeechOptions() { Locale = selectedLocale }, cancelToken: _cts.Token);
        }
        catch { }
        finally
        {
            DichGiaoDien();
            btnSpeak.BackgroundColor = Color.FromArgb("#2563EB");
            _cts?.Dispose(); _cts = null;
        }
    }
}