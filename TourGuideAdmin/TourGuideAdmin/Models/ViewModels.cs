namespace TourGuideAdmin.Models;

public class PoiViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int? Radius { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int ViewCount { get; set; }
    public int ListenCount { get; set; }
    public int PriorityScore { get; set; }
}

public class AudioViewModel
{
    public int Id { get; set; }
    public int? PoiId { get; set; }
    public string? PoiName { get; set; }
    public int? LanguageId { get; set; }
    public string? LanguageName { get; set; }
    public string? AudioUrl { get; set; }
    public int? Duration { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class TranslationViewModel
{
    public int Id { get; set; }
    public int? PoiId { get; set; }
    public string? PoiName { get; set; }
    public int? LanguageId { get; set; }
    public string? LanguageName { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class LanguageViewModel
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
}

public class UserViewModel
{
    public int Id { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Role { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class AudioLogViewModel
{
    public int Id { get; set; }
    public string? DeviceId { get; set; }
    public int? PoiId { get; set; }
    public string? PoiName { get; set; }
    public int? LanguageId { get; set; }
    public string? LanguageName { get; set; }
    public DateTime? PlayTime { get; set; }
}

public class AppSettingViewModel
{
    public int Id { get; set; }
    public string? SettingKey { get; set; }
    public string? SettingValue { get; set; }
}

public class DashboardViewModel
{
    public int TotalPOIs { get; set; }
    public int TotalAudios { get; set; }
    public int TotalTranslations { get; set; }
    public int TotalLanguages { get; set; }
    public int TotalUsers { get; set; }
    public int TotalAudioLogs { get; set; }
    public int ActiveUsersOnline { get; set; }
    public List<AudioLogViewModel> RecentLogs { get; set; } = new();
    public List<PoiViewModel> RecentPOIs { get; set; } = new();
    // 👉 2 Biến mới dùng để vẽ Biểu đồ Chart.js
    public List<string> TopPoiNames { get; set; } = new List<string>();
    public List<int> TopPoiListens { get; set; } = new List<int>();
}

public class QrCodeViewModel
{
    public int Id { get; set; }
    public int? PoiId { get; set; }
    public string? PoiName { get; set; }
    public string? QrValue { get; set; }
    public DateTime? CreatedAt { get; set; }
}
