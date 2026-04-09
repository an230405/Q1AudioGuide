using System.Text.Json;
using TourGuideApp.Models;

namespace TourGuideApp.Services;

public class ApiService
{
    HttpClient _httpClient;

    public ApiService()
    {
        var handler = new HttpClientHandler();
        // Bỏ qua kiểm tra chứng chỉ SSL
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
        _httpClient = new HttpClient(handler);
    }

    // ĐÃ THÊM THAM SỐ langId (0: VN, 1: EN, 2: CN, 3: JP)
    public async Task<List<POI>> GetPOIs(int langId = 0)
    {
        try
        {
            // BƯỚC 1: Dịch langId thành mã ngôn ngữ để gửi lên Server
            string langCode = "vi"; // Mặc định
            if (langId == 1) langCode = "en";
            else if (langId == 2) langCode = "zh";
            else if (langId == 3) langCode = "ja";

            // BƯỚC 2: Tự động nối mã ngôn ngữ vào link API
            string url = $"https://gwsmx4vm-7182.asse.devtunnels.ms/api/POI?lang={langCode}";

            // Ép Dev Tunnel trả về JSON, né trang web cảnh báo của Microsoft
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();

                // Nếu Dev Tunnel chưa Public, nó sẽ ném về 1 trang HTML chặn lại
                if (content.Contains("<html") || content.Contains("<!DOCTYPE"))
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await Application.Current.MainPage.DisplayAlert("Tunnel Bị Chặn!", "Link của bạn tải nhầm trang web. Đảm bảo Dev Tunnels đang để chế độ Public.", "OK");
                    });
                    return new List<POI>();
                }

                // Chuyển đổi JSON thành danh sách POI
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<List<POI>>(content, options);
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("Lỗi Server", $"Mã: {response.StatusCode}", "OK");
                });
            }
        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi Kết Nối", ex.Message, "OK");
            });
        }

        return new List<POI>();
    }
}