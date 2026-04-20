using Microsoft.AspNetCore.Mvc;
using TourGuideAdmin.Models;
using TourGuideAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using System.Text;

namespace TourGuideAdmin.Controllers;

[Authorize]
public class LanguageController : Controller
{
    private readonly ApiService _api;
    public LanguageController(ApiService api) => _api = api;

    public async Task<IActionResult> Index() => View(await _api.GetLanguagesAsync());
    public IActionResult Create() => View(new LanguageViewModel());

    [HttpPost]
    public async Task<IActionResult> Create(LanguageViewModel model)
    {
        var ok = await _api.CreateLanguageAsync(model);

        if (ok && !string.IsNullOrEmpty(model.Code))
        {
            await ProcessAutoTranslation(model.Code);
        }

        TempData[ok ? "Success" : "Error"] = ok ? "Thêm ngôn ngữ và tự động dịch thành công!" : "Lỗi khi thêm.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var lang = await _api.GetLanguageAsync(id);
        if (lang == null) return NotFound();
        return View(lang);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, LanguageViewModel model)
    {
        var ok = await _api.UpdateLanguageAsync(id, model);
        TempData[ok ? "Success" : "Error"] = ok ? "Cập nhật thành công!" : "Lỗi khi cập nhật.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _api.DeleteLanguageAsync(id);
        TempData[ok ? "Success" : "Error"] = ok ? "Xóa thành công!" : "Lỗi khi xóa.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ProcessAutoTranslation(string targetLangCode)
    {
        var languages = await _api.GetLanguagesAsync();
        var newLang = languages.FirstOrDefault(l => l.Code == targetLangCode);
        if (newLang == null) return;

        var pois = await _api.GetPOIsAsync();
        foreach (var poi in pois)
        {
            var translatedName = await TranslateText(poi.Name, targetLangCode);
            var translatedDesc = await TranslateText(poi.Description ?? "", targetLangCode);

            var translation = new TranslationViewModel
            {
                PoiId = poi.Id,
                LanguageId = newLang.Id,
                Title = translatedName,
                Content = translatedDesc
            };

            await _api.CreateTranslationAsync(translation);
        }
    }

    private async Task<string> TranslateText(string text, string targetLang)
    {
        if (string.IsNullOrWhiteSpace(text) || targetLang == "vi" || targetLang == "vn") return text;
        try
        {
            string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=auto&tl={targetLang}&dt=t&q={Uri.EscapeDataString(text)}";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
            string response = await client.GetStringAsync(url);

            var jsonDocument = JsonDocument.Parse(response);
            var sb = new StringBuilder();

            foreach (var item in jsonDocument.RootElement[0].EnumerateArray())
            {
                sb.Append(item[0].GetString());
            }
            return sb.ToString();
        }
        catch
        {
            return text;
        }
    }
}