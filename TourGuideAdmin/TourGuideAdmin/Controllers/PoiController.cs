using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TourGuideAdmin.Models;
using TourGuideAdmin.Services;

namespace TourGuideAdmin.Controllers;

[Authorize]
public class POIController : Controller
{
    private readonly ApiService _api;
    private readonly IWebHostEnvironment _env;   // 👉 ĐÃ THÊM: Khai báo biến môi trường
    private readonly IConfiguration _config;     // 👉 ĐÃ THÊM: Khai báo biến cấu hình

    // 👉 SỬA CONSTRUCTOR: Phải đưa env và config vào đây thì mới dùng được ở dưới
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
        // Gọi hàm lưu ảnh (Dòng 32 trong hình của Anh)
        model.ImageUrl = await SaveImageAsync(ImageFile, model.ImageUrl);

        var ok = await _api.CreatePOIAsync(model);
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
        // Gọi hàm lưu ảnh (Dòng 50 trong hình của Anh)
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

    // ============================================================
    // 👉 HÀM LƯU ẢNH: ĐÃ SỬA LỖI _config VÀ _env
    // ============================================================
    private async Task<string?> SaveImageAsync(IFormFile? file, string? existingUrl)
    {
        if (file == null || file.Length == 0) return existingUrl;

        // 1. ÉP ĐƯỜNG DẪN TUYỆT ĐỐI (Để chắc chắn sang đúng nhà API)
        string apiRelativePath = _config["ApiWebRoot"] ?? "..\\TourGuideAPI\\wwwroot";
        // Nhảy từ thư mục gốc của Admin sang API
        string apiWebRoot = Path.GetFullPath(Path.Combine(_env.ContentRootPath, apiRelativePath));

        var dir = Path.Combine(apiWebRoot, "images");

        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var path = Path.Combine(dir, fileName);

        using (var stream = new FileStream(path, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Trả về "images/tên-file.jpg"
        return $"images/{fileName}";
    }
}