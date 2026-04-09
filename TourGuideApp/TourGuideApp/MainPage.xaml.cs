namespace TourGuideApp;

public partial class MainPage : ContentPage
{
    readonly Services.ApiService _apiService = new Services.ApiService();
    List<Models.POI> _danhSachDiaDiem = new List<Models.POI>();

    public MainPage()
    {
        InitializeComponent();
    }

    private void OnLanguageChanged(object sender, EventArgs e)
    {
        var idx = langPicker.SelectedIndex;
        if (idx == 0) // Tiếng Việt
        {
            lblSubTitle.Text = "Người bạn đồng hành trên mọi nẻo đường";
            lblFeature1.Text = "Chỉ đường";
            lblFeature2.Text = "Thuyết minh";
            lblFeature3.Text = "Đa ngôn ngữ";
            btnStart.Text = "KHÁM PHÁ NGAY";
            lblLoading.Text = "Đang chuẩn bị hành trình...";
        }
        else if (idx == 1) // English
        {
            lblSubTitle.Text = "Your companion on every road";
            lblFeature1.Text = "Navigation";
            lblFeature2.Text = "Audio Guide";
            lblFeature3.Text = "Multi-lang";
            btnStart.Text = "EXPLORE NOW";
            lblLoading.Text = "Preparing journey...";
        }
        else if (idx == 2) // 中文
        {
            lblSubTitle.Text = "每条路上的伙伴";
            lblFeature1.Text = "导航";
            lblFeature2.Text = "语音导览";
            lblFeature3.Text = "多语言";
            btnStart.Text = "立即探索";
            lblLoading.Text = "正在准备行程...";
        }
        else if (idx == 3) // 日本語
        {
            lblSubTitle.Text = "あらゆる道のパートナー";
            lblFeature1.Text = "ナビ";
            lblFeature2.Text = "ガイド";
            lblFeature3.Text = "多言語";
            btnStart.Text = "今すぐ探索";
            lblLoading.Text = "準備中...";
        }
    }

    private async void OnStartClicked(object sender, EventArgs e)
    {
        if (langPicker.SelectedIndex == -1) return;

        App.CurrentLanguage = langPicker.SelectedIndex;
        loadingOverlay.IsVisible = true;
        btnStart.IsEnabled = false;

        var data = await _apiService.GetPOIs(App.CurrentLanguage);

        if (data != null && data.Count > 0)
        {
            string baseUrl = "https://gwsmx4vm-7182.asse.devtunnels.ms/";
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
            await DisplayAlert("Lỗi", "Không tải được dữ liệu!", "OK");
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