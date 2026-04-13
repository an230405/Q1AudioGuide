using System.Net.Http.Json;
using TourGuideAdmin.Models;

namespace TourGuideAdmin.Services;

public class ApiService
{
    private readonly HttpClient _http;

    public ApiService(IHttpClientFactory factory)
    {
        // Khởi tạo Client 1 lần duy nhất
        _http = factory.CreateClient("TourGuideAPI");

        // DÒNG QUAN TRỌNG: THẺ BÀI CHỐNG CHẶN DEV TUNNELS
        _http.DefaultRequestHeaders.Add("X-Tunnel-Skip-AntiPhishing-Page", "true");
    }

    // ── POI ──────────────────────────────────────────────────────────────────
    public async Task<List<PoiViewModel>> GetPOIsAsync()
        => await _http.GetFromJsonAsync<List<PoiViewModel>>("api/POI/all") ?? [];

    public async Task<PoiViewModel?> GetPOIAsync(int id)
        => await _http.GetFromJsonAsync<PoiViewModel>($"api/POI/{id}");

    public async Task<bool> CreatePOIAsync(PoiViewModel model)
    {
        var res = await _http.PostAsJsonAsync("api/POI", model);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> UpdatePOIAsync(int id, PoiViewModel model)
    {
        var res = await _http.PutAsJsonAsync($"api/POI/{id}", model);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> DeletePOIAsync(int id)
    {
        var res = await _http.DeleteAsync($"api/POI/{id}");
        return res.IsSuccessStatusCode;
    }

    // ── Audio ─────────────────────────────────────────────────────────────────
    public async Task<List<AudioViewModel>> GetAudiosAsync()
        => await _http.GetFromJsonAsync<List<AudioViewModel>>("api/Audio") ?? [];

    public async Task<AudioViewModel?> GetAudioAsync(int id)
        => await _http.GetFromJsonAsync<AudioViewModel>($"api/Audio/{id}");

    public async Task<bool> CreateAudioAsync(AudioViewModel model)
    {
        var res = await _http.PostAsJsonAsync("api/Audio", model);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAudioAsync(int id, AudioViewModel model)
    {
        var res = await _http.PutAsJsonAsync($"api/Audio/{id}", model);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAudioAsync(int id)
    {
        var res = await _http.DeleteAsync($"api/Audio/{id}");
        return res.IsSuccessStatusCode;
    }

    // ── Translation ───────────────────────────────────────────────────────────
    public async Task<List<TranslationViewModel>> GetTranslationsAsync()
        => await _http.GetFromJsonAsync<List<TranslationViewModel>>("api/Translation") ?? [];

    public async Task<TranslationViewModel?> GetTranslationAsync(int id)
        => await _http.GetFromJsonAsync<TranslationViewModel>($"api/Translation/{id}");

    public async Task<bool> CreateTranslationAsync(TranslationViewModel model)
    {
        var res = await _http.PostAsJsonAsync("api/Translation", model);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateTranslationAsync(int id, TranslationViewModel model)
    {
        var res = await _http.PutAsJsonAsync($"api/Translation/{id}", model);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteTranslationAsync(int id)
    {
        var res = await _http.DeleteAsync($"api/Translation/{id}");
        return res.IsSuccessStatusCode;
    }

    // ── Language ──────────────────────────────────────────────────────────────
    public async Task<List<LanguageViewModel>> GetLanguagesAsync()
        => await _http.GetFromJsonAsync<List<LanguageViewModel>>("api/Language") ?? [];

    public async Task<LanguageViewModel?> GetLanguageAsync(int id)
        => await _http.GetFromJsonAsync<LanguageViewModel>($"api/Language/{id}");

    public async Task<bool> CreateLanguageAsync(LanguageViewModel model)
    {
        var res = await _http.PostAsJsonAsync("api/Language", model);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateLanguageAsync(int id, LanguageViewModel model)
    {
        var res = await _http.PutAsJsonAsync($"api/Language/{id}", model);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteLanguageAsync(int id)
    {
        var res = await _http.DeleteAsync($"api/Language/{id}");
        return res.IsSuccessStatusCode;
    }

    // ── User ──────────────────────────────────────────────────────────────────
    public async Task<List<UserViewModel>> GetUsersAsync()
        => await _http.GetFromJsonAsync<List<UserViewModel>>("api/User") ?? [];

    public async Task<UserViewModel?> GetUserAsync(int id)
        => await _http.GetFromJsonAsync<UserViewModel>($"api/User/{id}");

    public async Task<bool> CreateUserAsync(UserViewModel model)
    {
        var res = await _http.PostAsJsonAsync("api/User", model);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateUserAsync(int id, UserViewModel model)
    {
        var res = await _http.PutAsJsonAsync($"api/User/{id}", model);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var res = await _http.DeleteAsync($"api/User/{id}");
        return res.IsSuccessStatusCode;
    }

    // ── AudioLog ──────────────────────────────────────────────────────────────
    public async Task<List<AudioLogViewModel>> GetAudioLogsAsync()
        => await _http.GetFromJsonAsync<List<AudioLogViewModel>>("api/AudioLog") ?? [];

    // ── AppSettings ───────────────────────────────────────────────────────────
    public async Task<List<AppSettingViewModel>> GetSettingsAsync()
        => await _http.GetFromJsonAsync<List<AppSettingViewModel>>("api/AppSetting") ?? [];

    public async Task<bool> UpdateSettingAsync(int id, AppSettingViewModel model)
    {
        var res = await _http.PutAsJsonAsync($"api/AppSetting/{id}", model);
        return res.IsSuccessStatusCode;
    }

    // ── Auth ──────────────────────────────────────────────────────────────────
    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/Auth/login",
                new { username, password });
            return res.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    // ── QrCode ────────────────────────────────────────────────────────────────
    public async Task<List<QrCodeViewModel>> GetQrCodesAsync()
        => await _http.GetFromJsonAsync<List<QrCodeViewModel>>("api/QrCode") ?? [];

    public async Task<bool> CreateQrCodeAsync(QrCodeViewModel model)
    {
        var res = await _http.PostAsJsonAsync("api/QrCode", new { model.PoiId, model.QrValue });
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteQrCodeAsync(int id)
    {
        var res = await _http.DeleteAsync($"api/QrCode/{id}");
        return res.IsSuccessStatusCode;
    }
}