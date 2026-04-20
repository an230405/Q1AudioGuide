namespace TourGuideApp.Services;

public static class AppTranslator
{
    public static Dictionary<string, string> TranslatedWords { get; set; } = new();

    // 👉 Anh kiểm tra và TỰ CHỈNH SỬA lại cái cột Tiếng Việt bên phải
    // sao cho nó KHỚP CHÍNH XÁC 100% với chữ đang hiện trên máy ảo nhé!
    private static readonly Dictionary<string, string> DefaultWords = new()
    {
        { "Greeting", "Xin chào! 👋" },
        { "Title", "Khám phá Sài Gòn" },
        { "Subtitle", "Người bạn đồng hành trên mọi nẻo đường" },
        { "Question", "Bạn muốn chọn ngôn ngữ nào?" },
        { "Nav", "Bản đồ" },
        { "Audio", "Thuyết minh" },
        { "Multi", "Đa ngôn ngữ" },
        { "Explore", "KHÁM PHÁ NGAY" },
        { "Loading", "Đang chuẩn bị hành trình..." },
        { "Near", "Địa điểm gần đây:" },

        { "Dist", "Cách bạn: {0} km" }, // 👉 DÒNG BỊ THIẾU ĐÂY ANH ƠI! THÊM NÓ VÀO NHÉ!

        { "Speak", "Nghe" },
        { "Home", "Trang chủ" },
        { "List", "Danh sách" },
        { "Qr", "Quét QR" }
    };

    public static async Task LoadTranslationsAsync(string langCode, ApiService api)
    {
        TranslatedWords.Clear();

        if (langCode == "vi" || langCode == "vn")
        {
            foreach (var item in DefaultWords) TranslatedWords[item.Key] = item.Value;
            return;
        }

        try
        {
            // 1. Gom tất cả chữ lại, nối với nhau bằng dấu ~
            var keys = DefaultWords.Keys.ToList();
            var values = DefaultWords.Values.ToList();
            string combinedText = string.Join(" ~ ", values);

            // 2. GỌI GOOGLE DỊCH ĐÚNG 1 LẦN!
            string translatedCombined = await api.GoogleTranslateAsync(combinedText, langCode);

            // 3. Cắt ra lại và nhét vào từ điển
            var translatedList = translatedCombined.Split('~').ToList();

            for (int i = 0; i < keys.Count; i++)
            {
                if (i < translatedList.Count)
                    TranslatedWords[keys[i]] = translatedList[i].Trim();
                else
                    TranslatedWords[keys[i]] = values[i]; // Lỗi thì trả về tiếng Việt
            }
        }
        catch
        {
            // Nếu mất mạng, giữ nguyên tiếng Việt
        }
    }

    public static string Get(string langCode, string key)
    {
        return TranslatedWords.ContainsKey(key) ? TranslatedWords[key] : DefaultWords.GetValueOrDefault(key, key);
    }
}