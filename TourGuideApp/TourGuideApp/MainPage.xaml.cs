namespace TourGuideApp;

public partial class MainPage : ContentPage
{
    readonly Services.ApiService _apiService = new Services.ApiService();
    List<Models.POI> _danhSachDiaDiem = new List<Models.POI>();

    // ĐÃ THÊM: Khai báo danh sách ngôn ngữ để không bị lỗi gạch đỏ
    List<Models.Language> _danhSachNgonNgu = new List<Models.Language>();

    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Lấy danh sách ngôn ngữ từ Admin API
        var langs = await _apiService.GetLanguages();
        if (langs != null && langs.Count > 0)
        {
            _danhSachNgonNgu = langs;
            langPicker.ItemsSource = _danhSachNgonNgu;
            // Chỉ hiển thị Tên (Vietnamese, Tiếng Thái) lên giao diện
            langPicker.ItemDisplayBinding = new Binding("Name");
        }
    }

    // KHI NGƯỜI DÙNG ĐỔI NGÔN NGỮ
    private void OnLanguageChanged(object sender, EventArgs e)
    {
        if (langPicker.SelectedIndex == -1 || _danhSachNgonNgu.Count == 0) return;

        var selectedLang = _danhSachNgonNgu[langPicker.SelectedIndex];

        App.CurrentLanguage = langPicker.SelectedIndex;
        App.CurrentLanguageCode = selectedLang.Code.ToLower();

        string lang = App.CurrentLanguageCode;

        // Đổi chữ trên màn hình ngay lập tức (Đã sửa đúng tên biến x:Name)
        lblGreeting.Text = Services.AppTranslator.Get(lang, "Greeting");
        lblTitle.Text = Services.AppTranslator.Get(lang, "Title");
        lblSubTitle.Text = Services.AppTranslator.Get(lang, "Subtitle"); // Chữ T viết hoa
        lblLangPrompt.Text = Services.AppTranslator.Get(lang, "Question"); // Sửa thành lblLangPrompt

        // Dịch 3 tính năng ở giữa màn hình
        lblFeature1.Text = Services.AppTranslator.Get(lang, "Nav");
        lblFeature2.Text = Services.AppTranslator.Get(lang, "Audio");
        lblFeature3.Text = Services.AppTranslator.Get(lang, "Multi");

        // Dịch nút bấm và màn hình chờ
        btnStart.Text = Services.AppTranslator.Get(lang, "Explore");
        lblLoading.Text = Services.AppTranslator.Get(lang, "Loading");
    }
    private async void OnStartClicked(object sender, EventArgs e)
    {
        if (langPicker.SelectedIndex == -1) return;

        App.CurrentLanguage = langPicker.SelectedIndex;
        loadingOverlay.IsVisible = true;
        btnStart.IsEnabled = false;

        // Lưu ý: Đã đổi truyền App.CurrentLanguageCode (ví dụ "th") vào thay vì truyền số
        // (Nếu file ApiService của Anh bị gạch đỏ chỗ này, Anh mở ApiService sửa tham số int thành string nhé)
        var data = await _apiService.GetPOIs(App.CurrentLanguageCode);

        if (data != null && data.Count > 0)
        {
            string baseUrl = "https://f8lzzzn0-7182.asse.devtunnels.ms/";
            foreach (var item in data)
            {
                if (!string.IsNullOrEmpty(item.ImageUrl) && !item.ImageUrl.StartsWith("http"))
                    item.ImageUrl = baseUrl + item.ImageUrl.TrimStart('/');
            }

            await TinhToanKhoangCach(data);
            _danhSachDiaDiem = data;

            loadingOverlay.IsVisible = false;
            btnStart.IsEnabled = true;

            await Navigation.PushAsync(new MapPage(_danhSachDiaDiem));
        }
        else
        {
            loadingOverlay.IsVisible = false;
            btnStart.IsEnabled = true;
            await DisplayAlert("Error", "Unable to load data!", "OK");
        }
    }

    private async Task TinhToanKhoangCach(List<Models.POI> list)
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (status == PermissionStatus.Granted)
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(5));
                var myLocation = await Geolocation.Default.GetLocationAsync(request);
                if (myLocation == null) myLocation = await Geolocation.Default.GetLastKnownLocationAsync();

                if (myLocation != null)
                {
                    foreach (var poi in list)
                    {
                        Location poiLoc = new Location(poi.Latitude, poi.Longitude);
                        poi.DistanceToUser = myLocation.CalculateDistance(poiLoc, DistanceUnits.Kilometers);
                    }
                    list.Sort((x, y) => x.DistanceToUser.CompareTo(y.DistanceToUser));
                }
            }
        }
        catch { }
    }
}