using System.Diagnostics;
using ZXing.Net.Maui;

namespace TourGuideApp;

public partial class QRScannerPage : ContentPage
{
    List<Models.POI> _danhSachPOI;
    bool _isScanning = true;

    public QRScannerPage(List<Models.POI> pois)
    {
        InitializeComponent();
        _danhSachPOI = pois;

        barcodeReader.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormat.QrCode,
            AutoRotate = true,
            Multiple = false
        };

        // GỌI HÀM DỊCH KHI MỞ CAMERA LÊN
        DichGiaoDien();
    }

    // ==========================================
    // HÀM TỰ ĐỘNG DỊCH NGÔN NGỮ CHO TRANG QR
    // ==========================================
    private void DichGiaoDien()
    {
        if (App.CurrentLanguage == 0) // Tiếng Việt
        {
            lblInstruction.Text = "📷 Đưa mã QR vào khung ngắm";
            lblHome.Text = "Trang chủ";
            lblQr.Text = "Quét QR";
        }
        else if (App.CurrentLanguage == 1) // Tiếng Anh
        {
            lblInstruction.Text = "📷 Align QR code within frame";
            lblHome.Text = "Home";
            lblQr.Text = "Scan QR";
        }
        else if (App.CurrentLanguage == 2) // Tiếng Trung
        {
            lblInstruction.Text = "📷 请将二维码放入框内";
            lblHome.Text = "首页";
            lblQr.Text = "扫码";
        }
        else // Tiếng Nhật
        {
            lblInstruction.Text = "📷 QRコードを枠内に配置";
            lblHome.Text = "ホーム";
            lblQr.Text = "QR読取";
        }
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

    private void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (!_isScanning) return;

        var firstResult = e.Results?.FirstOrDefault();
        if (firstResult != null)
        {
            _isScanning = false;
            string qrContent = firstResult.Value;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                barcodeReader.IsDetecting = false;

                var foundPoi = _danhSachPOI.FirstOrDefault(p => p.Id.ToString() == qrContent);

                if (foundPoi != null)
                {
                    await Navigation.PopAsync();
                    await Navigation.PushAsync(new PoiDetailPage(foundPoi));
                }
                else
                {
                    // Dịch luôn popup báo lỗi cho xịn
                    string errTitle = App.CurrentLanguage == 0 ? "Không tìm thấy" : (App.CurrentLanguage == 1 ? "Not Found" : (App.CurrentLanguage == 2 ? "未找到" : "見つかりません"));
                    string errMsg = App.CurrentLanguage == 0 ? $"Mã QR '{qrContent}' không thuộc khu du lịch này." : (App.CurrentLanguage == 1 ? $"Invalid QR code." : (App.CurrentLanguage == 2 ? $"无效的二维码" : $"無効なQRコード"));
                    string errBtn = App.CurrentLanguage == 0 ? "Quét lại" : (App.CurrentLanguage == 1 ? "Retry" : (App.CurrentLanguage == 2 ? "重试" : "再試行"));

                    await DisplayAlert(errTitle, errMsg, errBtn);
                    _isScanning = true;
                    barcodeReader.IsDetecting = true;
                }
            });
        }
    }

    // NÚT THOÁT VÀ NÚT HOME Ở FOOTER
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