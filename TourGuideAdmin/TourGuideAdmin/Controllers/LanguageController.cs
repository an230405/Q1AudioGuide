using Microsoft.AspNetCore.Mvc;
using TourGuideAdmin.Models;
using TourGuideAdmin.Services;
using Microsoft.AspNetCore.Authorization;

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
        TempData[ok ? "Success" : "Error"] = ok ? "Thêm ngôn ngữ thành công!" : "Lỗi khi thêm.";
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
}
