using System.Text.Json.Serialization;

namespace TourGuideApp.Models;

public class POI
{
    // ==========================================
    // PHẦN THÔNG TIN CƠ BẢN (Đã dọn dẹp trùng lặp)
    // ==========================================

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("radius")]
    public double Radius { get; set; }

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("content")]
    public Content? Content { get; set; }

    // Ánh xạ trực tiếp mô tả cấp độ cao nhất từ API (thường là tiếng Anh)
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    public int ViewCount { get; set; }
    public int ListenCount { get; set; }
    public int PriorityScore { get; set; }

    // ==========================================
    // PHẦN THÊM MỚI CHO TÍNH NĂNG ĐỊNH VỊ GPS
    // ==========================================

    [JsonIgnore] // Bỏ qua thuộc tính này khi đọc JSON từ API
    public double DistanceToUser { get; set; }

    [JsonIgnore] // Bỏ qua thuộc tính này khi đọc JSON từ API
    public string DistanceText
    {
        get
        {
            if (DistanceToUser <= 0)
                return "Đang định vị...";

            // Nếu dưới 1km thì hiển thị số mét (m), trên 1km thì hiển thị (km)
            if (DistanceToUser < 1)
                return $"📍 Cách đây {(DistanceToUser * 1000):F0} m";

            return $"📍 Cách đây {DistanceToUser:F1} km";
        }
    }


    // ==========================================
    // THUỘC TÍNH THÔNG MINH CHO GIAO DIỆN
    // ==========================================

    // ĐÂY LÀ DÒNG BỊ THIẾU LÀM NÓ BÁO LỖI NÈ ANH:
    private string? _finalDescription;

    // Ưu tiên lấy tiếng Việt trong Content, nếu rỗng thì mới lấy Description (tiếng Anh) ngoài cùng.
    [JsonIgnore]
    public string FinalDescription
    {
        get
        {
            // 1. Ưu tiên cao nhất: Nếu Google đã dịch và nhét vào hộp, thì lấy bản dịch ra xài!
            if (!string.IsNullOrEmpty(_finalDescription))
                return _finalDescription;

            // 2. Nếu không có bản dịch, thì lấy Content tiếng Việt như cũ
            if (Content != null && !string.IsNullOrEmpty(Content.Description))
                return Content.Description;

            // 3. Cuối cùng mới lấy tiếng Anh gốc
            return !string.IsNullOrEmpty(Description) ? Description : "(Chưa có nội dung thuyết minh)";
        }
        set
        {
            // CHO PHÉP GHI: Mở cửa để ApiService nhét bản dịch của Google vào đây
            _finalDescription = value;
        }
    }
}

public class Content
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("audioUrl")]
    public string? AudioUrl { get; set; }
}