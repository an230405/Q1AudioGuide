using System.Text.Json.Serialization;

public class POI
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("latitude")] public double Latitude { get; set; }
    [JsonPropertyName("longitude")] public double Longitude { get; set; }
    [JsonPropertyName("radius")] public double Radius { get; set; }
    [JsonPropertyName("imageUrl")] public string ImageUrl { get; set; }
    [JsonPropertyName("content")] public Content Content { get; set; }
}

public class Content
{
    [JsonPropertyName("title")] public string Title { get; set; }
    [JsonPropertyName("description")] public string Description { get; set; }
    [JsonPropertyName("audioUrl")] public string AudioUrl { get; set; }
}