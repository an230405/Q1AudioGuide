using System.Text.Json.Serialization;

namespace TourGuideApp.Models;

public class Language
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; } // Chứa mã "vi", "en", "th"...

    [JsonPropertyName("name")]
    public string Name { get; set; } // Chứa tên "Vietnamese", "Tiếng Thái"...
}