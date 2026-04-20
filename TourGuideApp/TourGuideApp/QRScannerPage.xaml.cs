using TourGuideApp.Services;
using ZXing.Net.Maui;

namespace TourGuideApp;

public partial class QRScannerPage : ContentPage
{
    bool _isScanning = true;
    ApiService _apiService;
    List<Models.POI> _danhSachPOI;

    // Sửa lại constructor để nhận dữ liệu POI truyền sang
    public QRScannerPage(List<Models.POI> pois = null)
    {
        InitializeComponent();
        _apiService = new ApiService();
        _danhSachPOI = pois ?? new List<Models.POI>(); // Bọc thép chống lỗi Null

        barcodeReader.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormat.QrCode,
            AutoRotate = true,
            Multiple = false
        };

        DichGiaoDien();
    }

    private void DichGiaoDien()
    {
        string lang = App.CurrentLanguageCode;

        lblInstruction.Text = Services.AppTranslator.Get(lang, "Instruction");
        lblHome.Text = Services.AppTranslator.Get(lang, "Home");
        lblQr.Text = Services.AppTranslator.Get(lang, "Qr");

        // Gọi dịch cho nút Danh sách
        lblList.Text = Services.AppTranslator.Get(lang, "List");
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        barcodeReader.IsDetecting = true;
        _isScanning = true;
    }

    protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
    {
        base.OnNavigatingFrom(args);
        barcodeReader.IsDetecting = false;
    }

    private async void OnBarcodesDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
    {
        var first = e.Results?.FirstOrDefault();
        if (first == null) return;

        Dispatcher.Dispatch(() => barcodeReader.IsDetecting = false);
        string qrContent = first.Value.Trim();

        if (int.TryParse(qrContent, out int poiId))
        {
            var detail = await _apiService.GetPOIById(poiId, App.CurrentLanguageCode);

            if (detail != null)
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status == PermissionStatus.Granted)
                {
                    var myLocation = await Geolocation.Default.GetLastKnownLocationAsync();
                    if (myLocation != null)
                    {
                        Location poiLoc = new Location(detail.Latitude, detail.Longitude);
                        detail.DistanceToUser = myLocation.CalculateDistance(poiLoc, DistanceUnits.Kilometers);
                    }
                }

                Dispatcher.Dispatch(async () =>
                {
                    await Navigation.PushAsync(new PoiDetailPage(detail));
                });
            }
            else
            {
                Dispatcher.Dispatch(() => ShowErrorAlert(qrContent, true));
            }
        }
        else
        {
            Dispatcher.Dispatch(() => ShowErrorAlert(qrContent, false));
        }
    }

    private async void ShowErrorAlert(string qrContent, bool isIdNotFound)
    {
        string lang = App.CurrentLanguageCode;
        string errTitle = Services.AppTranslator.Get(lang, "NotFound");
        string errMsg = isIdNotFound
                        ? $"{Services.AppTranslator.Get(lang, "NotFound")} ID: {qrContent}"
                        : Services.AppTranslator.Get(lang, "InvalidQR");
        string errBtn = Services.AppTranslator.Get(lang, "Retry");

        await DisplayAlert(errTitle, errMsg, errBtn);
        _isScanning = true;
        barcodeReader.IsDetecting = true;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        barcodeReader.IsDetecting = false;
        await Navigation.PopAsync();
    }

    private async void OnHomeClicked(object sender, TappedEventArgs e)
    {
        barcodeReader.IsDetecting = false;
        await Navigation.PopAsync(); // Trang chủ chính là trang Map nằm ở lớp dưới
    }

    // 👉 THÊM MỚI: Xử lý khi nhấn nút Danh sách
    private async void OnListClicked(object sender, TappedEventArgs e)
    {
        // 1. Tắt Camera ngay lập tức để tiết kiệm Pin
        barcodeReader.IsDetecting = false;

        // 2. Chuyển sang trang Danh Sách
        if (_danhSachPOI != null && _danhSachPOI.Any())
        {
            await Navigation.PushAsync(new PoiListPage(_danhSachPOI));
        }
        else
        {
            // Dự phòng: Nếu mất dữ liệu POI, quay về trang chủ để tải lại
            await Navigation.PopAsync();
        }
    }
}