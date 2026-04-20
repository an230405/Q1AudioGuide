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
        string cacheKey = $"pois_cache_{langCode}";

        try
        {
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                var response = await _httpClient.GetAsync($"api/POI?lang={langCode}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Preferences.Default.Set(cacheKey, content);

                    var data = JsonSerializer.Deserialize<List<Models.POI>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (data != null)
                    {
                        foreach (var item in data)
                        {
                            if (!string.IsNullOrEmpty(item.ImageUrl) && !item.ImageUrl.StartsWith("http"))
                            {
                                // 👉 FIX LỖI NỐI CHUỖI: Đảm bảo link chỉ có duy nhất 1 dấu / ở giữa
                                item.ImageUrl = _baseUrl.TrimEnd('/') + "/" + item.ImageUrl.TrimStart('/');
                            }
                        }
                    }
                    return data;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(@"Lỗi lấy dữ liệu Online: " + ex.Message);
        }

        // Chế độ Offline: Lấy từ kho lưu trữ
        var cachedData = Preferences.Default.Get(cacheKey, string.Empty);
        if (!string.IsNullOrEmpty(cachedData))
        {
            var data = JsonSerializer.Deserialize<List<Models.POI>>(cachedData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (data != null)
            {
                foreach (var item in data)
                {
                    if (!string.IsNullOrEmpty(item.ImageUrl) && !item.ImageUrl.StartsWith("http"))
                    {
                        item.ImageUrl = _baseUrl.TrimEnd('/') + "/" + item.ImageUrl.TrimStart('/');
                    }
                }
            }
            return data;
        }

        return new List<Models.POI>();
    }
    public async Task<Models.POI> GetPOIById(int id, string langCode = "vi")
    {
        try
        {
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                string url = $"api/POI/{id}?lang=vi";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var poi = JsonSerializer.Deserialize<Models.POI>(content, options);

                    if (poi != null)
                    {
                        if (!string.IsNullOrEmpty(poi.ImageUrl) && !poi.ImageUrl.StartsWith("http"))
                        {
                            // 👉 FIX LỖI NỐI CHUỖI: Dùng _baseUrl chung để đồng bộ
                            poi.ImageUrl = _baseUrl.TrimEnd('/') + "/" + poi.ImageUrl.TrimStart('/');
                        }

                        string originText = poi.Content?.Description ?? poi.Description ?? "Chưa có nội dung";

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
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Lỗi lấy chi tiết POI Online: {ex.Message}");
        }

        // Nếu lỗi hoặc mất mạng -> Tìm trong danh sách đã lưu ở kho
        var allPois = await GetPOIs(langCode);
        var offlinePoi = allPois.FirstOrDefault(p => p.Id == id);

        if (offlinePoi != null)
        {
            if (!string.IsNullOrEmpty(offlinePoi.ImageUrl) && !offlinePoi.ImageUrl.StartsWith("http"))
            {
                offlinePoi.ImageUrl = _baseUrl.TrimEnd('/') + "/" + offlinePoi.ImageUrl.TrimStart('/');
            }
            offlinePoi.FinalDescription = offlinePoi.Content?.Description ?? offlinePoi.Description ?? "Chưa có nội dung";
        }

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
    // Đặt cái này ở cuối file ApiService.cs
    public class DynamicDictionary
    {
        // Kho chứa từ đã dịch
        public static Dictionary<string, string> TranslatedWords { get; set; } = new();

        // 1. CHUẨN BỊ 10 TỪ "LẶT VẶT" CẦN DỊCH TRÊN APP
        private static readonly Dictionary<string, string> DefaultWords = new()
    {
        { "Nearby", "Địa điểm gần bạn:" },
        { "Distance", "Cách bạn" },
        { "PlayAudio", "PHÁT THUYẾT MINH" },
        { "Home", "Trang chủ" },
        { "List", "Danh sách" },
        { "ScanQR", "Quét QR" },
        { "Km", "km" },
        { "Loading", "Đang tải..." }
    };

        // 2. HÀM TỰ ĐỘNG DỊCH 1 LẦN RỒI NHỚ LUÔN
        public static async Task LoadTranslationsAsync(string langCode, ApiService api)
        {
            TranslatedWords.Clear();

            // Nếu là tiếng Việt thì khỏi dịch, xài luôn từ gốc
            if (langCode == "vi" || langCode == "vn")
            {
                foreach (var item in DefaultWords) TranslatedWords[item.Key] = item.Value;
                return;
            }

            // Nếu là tiếng nước ngoài thì mang đi dịch
            foreach (var item in DefaultWords)
            {
                // Gọi hàm Google Dịch đã có sẵn của Anh
                string translated = await api.GoogleTranslateAsync(item.Value, langCode);
                TranslatedWords[item.Key] = translated;
            }
        }

        // 3. HÀM LẤY CHỮ RA XÀI NHANH
        public static string Get(string key)
        {
            return TranslatedWords.ContainsKey(key) ? TranslatedWords[key] : DefaultWords.GetValueOrDefault(key, key);
        }
    }
}