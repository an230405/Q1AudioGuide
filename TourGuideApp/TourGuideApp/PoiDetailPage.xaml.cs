using TourGuideApp.Models;
using System.Xml;


namespace TourGuideApp;

public partial class PoiDetailPage : ContentPage
{
    // Đây là hàm khởi tạo, nhận vào dữ liệu của địa điểm được chọn
    public PoiDetailPage(POI selectedPoi)
    {
        InitializeComponent();

        // Đổ dữ liệu từ 'selectedPoi' vào giao diện
        BindingContext = selectedPoi;

        // Hoặc gán thủ công nếu bạn đã đặt x:Name cho các Label
        NameLabel.Text = selectedPoi.Name;
        PoiImage.Source = selectedPoi.ImageUrl;

        if (selectedPoi.Content != null)
        {
            TitleLabel.Text = selectedPoi.Content.Title;
            DescriptionLabel.Text = selectedPoi.Content.Description;
        }
    }
    private CancellationTokenSource cts; // Dùng để dừng giọng nói giữa chừng

    private async void OnSpeakClicked(object sender, EventArgs e)
    {
        var poi = BindingContext as POI;
        if (poi?.Content?.Description == null) return;

        var button = sender as Button;

        try
        {
            // Nếu đang đọc mà nhấn lại thì sẽ DỪNG
            if (cts?.IsCancellationRequested == false)
            {
                cts.Cancel();
                button.Text = "🔊 Nghe thuyết minh";
                return;
            }

            cts = new CancellationTokenSource();
            button.Text = "🛑 Dừng thuyết minh";

            // Lấy danh sách giọng đọc và chọn Tiếng Việt
            // Lấy danh sách giọng đọc
            var locales = await TextToSpeech.Default.GetLocalesAsync();

            // Tìm giọng Tiếng Việt (chứa chữ "vi" không phân biệt hoa thường)
            var viLocale = locales.FirstOrDefault(l => l.Language.ToLower().Contains("vi"));

            // Cảnh báo nếu máy không có tiếng Việt
            if (viLocale == null)
            {
                await DisplayAlert("Báo cáo", "Điện thoại của bạn chưa tải Giọng đọc Tiếng Việt, máy sẽ đọc tạm bằng tiếng Anh nhé!", "OK");
            }

            var options = new SpeechOptions()
            {
                Locale = viLocale, // Nếu không có viLocale, nó sẽ dùng giọng mặc định
                Pitch = 1.0f,
                Volume = 1.0f
            };

            // Truyền options vào hàm SpeakAsync
            await TextToSpeech.Default.SpeakAsync(poi.Content.Description, options, cancelToken: cts.Token);
        }
        catch (Exception)
        {
            // Xử lý khi nhấn nút dừng hoặc lỗi
        }
        finally
        {
            button.Text = "🔊 Nghe thuyết minh";
            cts?.Dispose();
            cts = null;
        }
    }
}