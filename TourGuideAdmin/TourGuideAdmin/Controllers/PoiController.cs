using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TourGuideAdmin.Models;
using TourGuideAdmin.Services;

namespace TourGuideAdmin.Controllers;

[Authorize]
public class POIController : Controller
{
    private readonly ApiService _api;
    private readonly IWebHostEnvironment _env;

    public POIController(ApiService api, IWebHostEnvironment env)
    {
        _api = api;
        _env = env;
    }

    public async Task<IActionResult> Index()
        => View(await _api.GetPOIsAsync());

    public IActionResult Create() => View(new PoiViewModel { IsActive = true, Radius = 30 });

    [HttpPost]
    public async Task<IActionResult> Create(PoiViewModel model, IFormFile? ImageFile)
    {
        model.ImageUrl = await SaveImageAsync(ImageFile, model.ImageUrl);
        var ok = await _api.CreatePOIAsync(model);
        TempData[ok ? "Success" : "Error"] = ok ? "Thêm địa điểm thành công!" : "Lỗi khi thêm địa điểm.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var poi = await _api.GetPOIAsync(id);
        if (poi == null) return NotFound();
        return View(poi);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, PoiViewModel model, IFormFile? ImageFile)
    {
        model.ImageUrl = await SaveImageAsync(ImageFile, model.ImageUrl);
        var ok = await _api.UpdatePOIAsync(id, model);
        TempData[ok ? "Success" : "Error"] = ok ? "Cập nhật thành công!" : "Lỗi khi cập nhật.";
        return RedirectToAction(nameof(Index));
    }

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
        var dir = Path.Combine(_env.WebRootPath, "images");
        Directory.CreateDirectory(dir);
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var path = Path.Combine(dir, fileName);
        using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream);
        return $"/images/{fileName}";
    }
}
