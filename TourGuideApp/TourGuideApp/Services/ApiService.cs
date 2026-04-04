using System.Text.Json;
using TourGuideApp.Models;

namespace TourGuideApp.Services;

public class ApiService
{
    HttpClient _httpClient;

    public ApiService()
    {
        // Giữ lại phần này để Windows Machine chấp nhận link https://localhost
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
        _httpClient = new HttpClient(handler);
    }

    public async Task<List<POI>> GetPOIs()
    {
        try
        {
            // ĐỔI THÀNH api/POI (Viết hoa, không có s)
            string url = DeviceInfo.Platform == DevicePlatform.Android
                       ? "https://10.0.2.2:7182/api/POI?lang=vi"
                       : "https://localhost:7182/api/POI?lang=vi";

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<POI>>(json);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"--- LỖI SERVER: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"--- LỖI KẾT NỐI: {ex.Message}");
        }
        return new List<POI>();
    }
}