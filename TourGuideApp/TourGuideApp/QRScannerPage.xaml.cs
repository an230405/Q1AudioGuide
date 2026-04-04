using TourGuideApp.Models;
using ZXing.Net.Maui;

namespace TourGuideApp;

public partial class QRScannerPage : ContentPage
{
    List<POI> _danhSachDiaDiem;

    public QRScannerPage(List<POI> data)
    {
        InitializeComponent();
        _danhSachDiaDiem = data;

        // Ép nó chỉ tập trung tìm mã QR (Bỏ qua mã vạch siêu thị để quét cho lẹ)
        barcodeReader.Options = new BarcodeReaderOptions
        {
            Formats = ZXing.Net.Maui.BarcodeFormat.QrCode,
            AutoRotate = true,
            Multiple = false
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        barcodeReader.IsDetecting = true; // Mở trang lên là tự động quét
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        barcodeReader.IsDetecting = false; // Tắt trang thì dừng quét cho đỡ tốn pin
    }

    // Hàm này chạy khi camera chụp được mã
    private void BarcodeReader_BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        var result = e.Results?.FirstOrDefault();
        if (result != null)
        {
            barcodeReader.IsDetecting = false; // Thấy mã là phanh gấp lại ngay

            Dispatcher.DispatchAsync(async () =>
            {
                string qrText = result.Value; // Lấy chữ bên trong mã QR
                var foundPoi = _danhSachDiaDiem.FirstOrDefault(p => p.Name == qrText);

                if (foundPoi != null)
                {
                    // Tìm thấy điểm du lịch -> Mở trang chi tiết
                    await Navigation.PushAsync(new PoiDetailPage(foundPoi));
                }
                else
                {
                    // Không tìm thấy
                    await DisplayAlert("Chú ý", $"Không tìm thấy dữ liệu cho mã: {qrText}", "OK");
                    barcodeReader.IsDetecting = true; // Cho quét lại
                }
            });
        }
    }
}