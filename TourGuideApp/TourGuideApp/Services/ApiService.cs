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
        string cacheKey = $"pois_cache_{langCode}"; // Chìa khóa để mở kho dữ liệu theo ngôn ngữ

        try
        {
            // 1. KIỂM TRA MẠNG TRƯỚC
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                // CÓ MẠNG: Gọi API lấy dữ liệu mới nhất
                var response = await _httpClient.GetAsync($"api/POI?lang={langCode}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    // 👉 CẤT VÀO KHO (CACHE): Lưu chuỗi JSON này vào ổ cứng điện thoại
                    Preferences.Default.Set(cacheKey, content);

                    return JsonSerializer.Deserialize<List<Models.POI>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(@"Lỗi lấy dữ liệu Online: " + ex.Message);
        }

        // 2. CHẾ ĐỘ OFFLINE: Nếu rớt mạng hoặc API sập, lôi hàng trong kho ra
        var cachedData = Preferences.Default.Get(cacheKey, string.Empty);

        if (!string.IsNullOrEmpty(cachedData))
        {
            // Có dữ liệu cũ, giải nén và trả về cho App dùng tạm
            return JsonSerializer.Deserialize<List<Models.POI>>(cachedData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        // Nếu cả kho cũng trống (lần đầu mở app mà không có mạng) thì đành chịu
        return new List<Models.POI>();
    }

    // Đã sửa tham số thành chuỗi string "vi" làm mặc định
    public async Task<Models.POI> GetPOIById(int id, string langCode = "vi")
    {
        try
        {
            // 1. CHỈ GỌI SERVER & GOOGLE DỊCH KHI CÓ INTERNET
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
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

                        // DỊCH SANG TIẾNG HÀN/TRUNG/NHẬT... (Google Dịch bắt buộc phải có mạng)
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
                    return poi; // Xử lý xong, trả về luôn
                }
            }
        }
        catch (Exception ex)
        {
            // Bỏ DisplayAlert đi để không làm phiền khách. Ghi log ngầm thôi.
            Debug.WriteLine($"Lỗi lấy chi tiết POI Online: {ex.Message}");
        }

        // ========================================================
        // 2. CHẾ ĐỘ OFFLINE: NẾU RỚT MẠNG HOẶC LỖI -> TÌM TRONG KHO
        // ========================================================

        // Gọi hàm GetPOIs (vì không có mạng, nó sẽ tự động moi danh sách từ ổ cứng điện thoại ra)
        var allPois = await GetPOIs(langCode);
        var offlinePoi = allPois.FirstOrDefault(p => p.Id == id);

        if (offlinePoi != null)
        {
            // Xử lý lại đường dẫn ảnh cho nó khỏi bị lỗi
            if (!string.IsNullOrEmpty(offlinePoi.ImageUrl) && !offlinePoi.ImageUrl.StartsWith("http"))
                offlinePoi.ImageUrl = "https://f8lzzzn0-7182.asse.devtunnels.ms/" + offlinePoi.ImageUrl.TrimStart('/');

            // Rút nội dung ra để gán vào FinalDescription cho nút Audio nó đọc
            offlinePoi.FinalDescription = offlinePoi.Content?.Description ?? offlinePoi.Description ?? "Chưa có nội dung";
        }

        // Trả về địa điểm lấy từ trong kho
        return offlinePoi;
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
    public async Task IncreaseViewAsync(int poiId)
    {
        try
        {
            var content = new StringContent("", System.Text.Encoding.UTF8, "application/json");
            await _httpClient.PostAsync($"api/POI/{poiId}/increase-view", content);
        }
        catch { /* Nuốt lỗi ngầm để không làm phiền người dùng nếu rớt mạng */ }
    }

    // Hàm bắn tín hiệu khi người dùng bấm nút Nghe Audio
    public async Task IncreaseListenAsync(int poiId)
    {
        try
        {
            var content = new StringContent("", System.Text.Encoding.UTF8, "application/json");
            await _httpClient.PostAsync($"api/POI/{poiId}/increase-listen", content);
        }
        catch { /* Tương tự, nếu gọi API xịt thì thôi, app vẫn chạy bình thường */ }
    }
    // Hàm 1: Cộng lượt nghe
    // Gửi tín hiệu nhịp tim về Server
    public async Task PingTrackingAsync(string deviceId)
    {
        try
        {
            // Gửi ID của điện thoại lên để Server biết ai đang online
            await _httpClient.PostAsync($"api/Tracking/ping?deviceId={deviceId}", null);
        }
        catch
        {
            // Nếu rớt mạng gửi xịt thì thôi, hệ thống bắt mạng bên dưới sẽ tự lo
        }
    }
}