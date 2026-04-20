using static Android.Graphics.BlurMaskFilter;

namespace TourGuideApp;

public partial class PoiListPage : ContentPage
{
    private List<Models.POI> _allPois;
    private bool _isNavigating = false;

    public PoiListPage(List<Models.POI> pois)
    {
        InitializeComponent();

        string lang = App.CurrentLanguageCode ?? "vi";

        // Dịch các chữ trên giao diện
        searchBar.Placeholder = Services.AppTranslator.Get(lang, "Search") ?? "Tìm kiếm...";
        lblHome.Text = Services.AppTranslator.Get(lang, "Home");
        lblList.Text = Services.AppTranslator.Get(lang, "List");
        lblQr.Text = Services.AppTranslator.Get(lang, "Qr");

        // Sắp xếp danh sách
        _allPois = pois.OrderBy(p => p.DistanceToUser).ToList();
        poiCollectionView.ItemsSource = _allPois;
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var keyword = e.NewTextValue?.ToLower() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(keyword))
        {
            poiCollectionView.ItemsSource = _allPois;
        }
        else
        {
            poiCollectionView.ItemsSource = _allPois.Where(p => p.Name.ToLower().Contains(keyword)).ToList();
        }
    }

    private async void OnFrameTapped(object sender, TappedEventArgs e)
    {
        if (_isNavigating) return;
        _isNavigating = true;

        if (sender is Frame frame && frame.BindingContext is Models.POI selectedPoi)
        {
            // Hiệu ứng mờ nháy nhẹ
            await frame.FadeTo(0.5, 100);
            await frame.FadeTo(1, 100);

            await Navigation.PushAsync(new PoiDetailPage(selectedPoi));
        }

        _isNavigating = false;
    }

    // 👉 THÊM MỚI: Bấm Trang chủ -> Lùi về MapPage (Trang gốc)
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

    // 👉 THÊM MỚI: Bấm Quét QR -> Mở trang QR và truyền dữ liệu đi
    private async void OnFooterQrTapped(object sender, TappedEventArgs e)
    {
        if (_isNavigating) return;
        _isNavigating = true;
        await Navigation.PushAsync(new QRScannerPage(_allPois));
        _isNavigating = false;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _isNavigating = false;
    }
}