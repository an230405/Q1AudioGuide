namespace TourGuideApp;

public partial class MainPage : ContentPage
{
    readonly Services.ApiService _apiService = new Services.ApiService();

    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnLoadClicked(object sender, EventArgs e)
    {
        var data = await _apiService.GetPOIs();

        if (data != null && data.Count > 0)
        {
            string baseUrl = DeviceInfo.Platform == DevicePlatform.Android
                             ? "https://10.0.2.2:7182/"
                             : "https://localhost:7182/";

            foreach (var item in data)
            {
                // Kiểm tra và nối chuỗi cho ImageUrl
                if (!string.IsNullOrEmpty(item.ImageUrl) && !item.ImageUrl.StartsWith("http"))
                {
                    // Xóa dấu gạch chéo dư thừa nếu có
                    string cleanImageUrl = item.ImageUrl.TrimStart('/');
                    item.ImageUrl = baseUrl + cleanImageUrl;
                }
            }

            poiList.ItemsSource = data;
        }
        else
        {
            await DisplayAlert("Lỗi", "Vẫn không thấy dữ liệu! Hãy đảm bảo bạn đã nhập dữ liệu vào SQL Server.", "OK");
        }
    }
    private async void OnPoiSelected(object sender, SelectionChangedEventArgs e)
    {
        var item = e.CurrentSelection.FirstOrDefault() as Models.POI;
        if (item == null) return;

        await Navigation.PushAsync(new PoiDetailPage(item));
        ((CollectionView)sender).SelectedItem = null;
    }
}