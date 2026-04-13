using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TourGuideAdmin.Models;
using TourGuideAdmin.Services;
using Microsoft.AspNetCore.Authorization;

namespace TourGuideAdmin.Controllers;

[Authorize]
public class TranslationController : Controller
{
    private readonly ApiService _api;
    public TranslationController(ApiService api) => _api = api;

    public async Task<IActionResult> Index()
        => View(await _api.GetTranslationsAsync());

    private async Task PopulateDropdowns(int? selectedPoi = null, int? selectedLang = null)
    {
        var pois = await _api.GetPOIsAsync();
        var langs = await _api.GetLanguagesAsync();
        ViewBag.Pois = new SelectList(pois, "Id", "Name", selectedPoi);
        ViewBag.Languages = new SelectList(langs, "Id", "Name", selectedLang);
    }

    public async Task<IActionResult> Create() { await PopulateDropdowns(); return View(new TranslationViewModel()); }

    [HttpPost]
    public async Task<IActionResult> Create(TranslationViewModel model)
    {
        if (!ModelState.IsValid) { await PopulateDropdowns(model.PoiId, model.LanguageId); return View(model); }
        var ok = await _api.CreateTranslationAsync(model);
        TempData[ok ? "Success" : "Error"] = ok ? "Thêm bản dịch thành công!" : "Lỗi khi thêm.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var t = await _api.GetTranslationAsync(id);
        if (t == null) return NotFound();
        await PopulateDropdowns(t.PoiId, t.LanguageId);
        return View(t);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, TranslationViewModel model)
    {
        if (!ModelState.IsValid) { await PopulateDropdowns(model.PoiId, model.LanguageId); return View(model); }
        var ok = await _api.UpdateTranslationAsync(id, model);
        TempData[ok ? "Success" : "Error"] = ok ? "Cập nhật thành công!" : "Lỗi khi cập nhật.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _api.DeleteTranslationAsync(id);
        TempData[ok ? "Success" : "Error"] = ok ? "Xóa thành công!" : "Lỗi khi xóa.";
        return RedirectToAction(nameof(Index));
    }
}
