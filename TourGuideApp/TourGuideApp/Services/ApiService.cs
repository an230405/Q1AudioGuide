using System.Text.Json;
using System.Diagnostics;
using TourGuideApp.Models;

namespace TourGuideApp.Services;

public class ApiService
{
    HttpClient _httpClient;
    string _baseUrl = "https://f8lzzzn0-7182.asse.devtunnels.ms/";

    public ApiService()
    {
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
        _httpClient = new HttpClient(handler);
        _httpClient.BaseAddress = new Uri(_baseUrl);

        _httpClient.DefaultRequestHeaders.Add("X-Tunnel-Skip-AntiPhishing-Page", "true");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task<List<Models.POI>> GetPOIs(string langCode = "vi")
    {
        try
        {
            // Gọi thẳng API với đuôi ?lang=th hoặc ?lang=vi
            var response = await _httpClient.GetAsync($"api/POI?lang={langCode}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Models.POI>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
        }
        catch { }
        return new List<Models.POI>();
    }

    // Đã sửa tham số thành chuỗi string "vi" làm mặc định
    public async Task<Models.POI> GetPOIById(int id, string langCode = "vi")
    {
        try
        {
            // 1. ÉP HỆ THỐNG XIN BẢN TIẾNG VIỆT TỪ SERVER (Vì tiếng Việt luôn có bài dài nhất)
            // LƯU Ý: Nếu Server của Anh cài đặt tiếng Việt là "vn" thì đổi chữ "vi" dưới đây thành "vn" nhé
            string url = $"api/POI/{id}?lang=vi";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var poi = System.Text.Json.JsonSerializer.Deserialize<Models.POI>(content, options);

                if (poi != null)
                {
                    if (!string.IsNullOrEmpty(poi.ImageUrl) && !poi.ImageUrl.StartsWith("http"))
                        poi.ImageUrl = "https://f8lzzzn0-7182.asse.devtunnels.ms/" + poi.ImageUrl.TrimStart('/');

                    // Lấy bài dài, nếu server vẫn giấu thì đành lấy bài ngắn
                    string originText = poi.Content?.Description ?? poi.Description ?? "Chưa có nội dung";

                    // 2. DỊCH SANG TIẾNG HÀN/TRUNG/NHẬT...
                    if (langCode != "vi" && langCode != "vn")
                    {
                        poi.Name = await GoogleTranslateAsync(poi.Name, langCode);
                        poi.FinalDescription = await GoogleTranslateAsync(originText, langCode);
                    }
                    else
                    {
                        poi.FinalDescription = originText;
                    }
                }
                return poi;
            }
        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi API", ex.Message, "OK");
            });
        }
        return null;
    }

    // ==================================================
    // BỘ DỊCH TỰ ĐỘNG ĐÃ ĐƯỢC NÂNG CẤP CHỐNG CHẶN
    // ==================================================
    public async Task<string> GoogleTranslateAsync(string text, string targetLangCode)
    {
        if (string.IsNullOrWhiteSpace(text) || targetLangCode == "vi" || targetLangCode == "vn") return text;

        try
        {
            // Dùng sl=auto để nó tự biết chữ đang đưa vào là tiếng Anh hay Việt
            string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=auto&tl={targetLangCode}&dt=t&q={Uri.EscapeDataString(text)}";

            using var client = new HttpClient();

            // CHÌA KHÓA VÀNG: Cải trang thành Trình duyệt Web để Google không chặn
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

            string response = await client.GetStringAsync(url);

            var jsonDocument = System.Text.Json.JsonDocument.Parse(response);
            var sb = new System.Text.StringBuilder();

            foreach (var item in jsonDocument.RootElement[0].EnumerateArray())
            {
                sb.Append(item[0].GetString());
            }
            return sb.ToString();
        }
        catch
        {
            // Nếu Google vẫn chặn hoặc rớt mạng, nó sẽ hiện đuôi "(Lỗi Dịch)" để mình biết!
            return text + " (Lỗi Dịch)";
        }
    }

    public async Task<List<Models.Language>> GetLanguages()
    {
        try
        {
            // Thay "api/Language" bằng đúng đường dẫn API chứa ngôn ngữ của Anh nếu khác nhé
            var response = await _httpClient.GetAsync("api/Language");
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return System.Text.Json.JsonSerializer.Deserialize<List<Models.Language>>(content, options);
            }
        }
        catch
        {
            // Nếu lỗi mạng, trả về danh sách rỗng để không bị sập App
        }
        return new List<Models.Language>();
    }
}