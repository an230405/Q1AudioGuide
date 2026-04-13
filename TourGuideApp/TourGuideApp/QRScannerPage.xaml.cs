using TourGuideApp.Services;
using ZXing.Net.Maui;

namespace TourGuideApp;

public partial class QRScannerPage : ContentPage
{
    bool 
        _isScanning = true;
    ApiService _apiService;

    public QRScannerPage()
    {
        InitializeComponent();
        _apiService = new ApiService();

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
        string lang = App.CurrentLanguageCode; // Lấy "th", "vi", "en"...

        // Gọi Từ điển ra xài, code sạch đẹp rạng ngời!
        lblInstruction.Text = Services.AppTranslator.Get(lang, "Instruction");
        lblHome.Text = Services.AppTranslator.Get(lang, "Home");
        lblQr.Text = Services.AppTranslator.Get(lang, "Qr");
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
            // Sửa tham số truyền vào API thành App.CurrentLanguageCode (VD: "th")
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

    // Hàm báo lỗi đã được làm lại bằng Từ điển
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
        await Navigation.PopAsync();
    }
}