namespace TourGuideApp.Models;

using System.Text.Json.Serialization;

public class POI
{
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
            if (DistanceToUser == 0)
                return "Đang định vị...";

            // Nếu dưới 1km thì hiển thị số mét (m), trên 1km thì hiển thị (km)
            if (DistanceToUser < 1)
                return $"📍 Cách đây {(DistanceToUser * 1000):F0} m";

            return $"📍 Cách đây {DistanceToUser:F1} km";
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