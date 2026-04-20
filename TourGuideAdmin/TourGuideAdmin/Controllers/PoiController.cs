using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TourGuideAdmin.Models;
using TourGuideAdmin.Services;
using System.Text.Json;
using System.Text;

namespace TourGuideAdmin.Controllers;

[Authorize]
public class POIController : Controller
{
    private readonly ApiService _api;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;

    public POIController(ApiService api, IWebHostEnvironment env, IConfiguration config)
    {
        _api = api;
        _env = env;
        _config = config;
    }

    public async Task<IActionResult> Index()
        => View(await _api.GetPOIsAsync());

    [Authorize(Roles = "admin,Admin")]
    public IActionResult Create() => View(new PoiViewModel { IsActive = true, Radius = 30 });

    [Authorize(Roles = "admin,Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(PoiViewModel model, IFormFile? ImageFile)
    {
        model.ImageUrl = await SaveImageAsync(ImageFile, model.ImageUrl);
        var ok = await _api.CreatePOIAsync(model);

        if (ok)
        {
            await AutoTranslateNewPoiAsync(model.Name);
        }

        TempData[ok ? "Success" : "Error"] = ok ? "Thêm địa điểm thành công!" : "Lỗi khi thêm địa điểm.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "admin,Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var poi = await _api.GetPOIAsync(id);
        if (poi == null) return NotFound();
        return View(poi);
    }

    [Authorize(Roles = "admin,Admin")]
    [HttpPost]
    public async Task<IActionResult> Edit(int id, PoiViewModel model, IFormFile? ImageFile)
    {
        model.ImageUrl = await SaveImageAsync(ImageFile, model.ImageUrl);
        var ok = await _api.UpdatePOIAsync(id, model);
        TempData[ok ? "Success" : "Error"] = ok ? "Cập nhật thành công!" : "Lỗi khi cập nhật.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "admin,Admin")]
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _api.DeletePOIAsync(id);
        TempData[ok ? "Success" : "Error"] = ok ? "Xóa thành công!" : "Lỗi khi xóa.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<string?> SaveImageAsync(IFormFile? file, string? existingUrl)
    {
        if (file == null || file.Length == 0) return existingUrl;

        // 1. Lấy đường dẫn từ appsettings.json
        string apiWebRoot = _config["ApiWebRoot"];

        // 2. Nếu trong appsettings.json không có, thì nó mới dùng đường dẫn tương đối (dễ lỗi)
        if (string.IsNullOrEmpty(apiWebRoot))
        {
            apiWebRoot = Path.GetFullPath(Path.Combine(_env.ContentRootPath, "..\\TourGuideAPI\\wwwroot"));
        }

        var dir = Path.Combine(apiWebRoot, "images");

        // Tự động tạo thư mục images nếu bên API chưa có
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var path = Path.Combine(dir, fileName);

        using (var stream = new FileStream(path, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Trả về chuỗi để lưu vào DB: "images/tên-file.jpg"
        return $"images/{fileName}";
    }

    private async Task AutoTranslateNewPoiAsync(string poiName)
    {
        var allPois = await _api.GetPOIsAsync();
        var newPoi = allPois.OrderByDescending(p => p.Id).FirstOrDefault(p => p.Name == poiName);

        if (newPoi == null) return;

        var languages = await _api.GetLanguagesAsync();

        foreach (var lang in languages)
        {
            if (lang.Code == "vi" || lang.Code == "vn") continue;

            var translatedName = await TranslateTextAsync(newPoi.Name, lang.Code);
            var translatedDesc = await TranslateTextAsync(newPoi.Description ?? "", lang.Code);

            var translation = new TranslationViewModel
            {
                PoiId = newPoi.Id,
                LanguageId = lang.Id,
                Title = translatedName,
                Content = translatedDesc
            };

            await _api.CreateTranslationAsync(translation);
        }
    }

    private async Task<string> TranslateTextAsync(string text, string targetLang)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;
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