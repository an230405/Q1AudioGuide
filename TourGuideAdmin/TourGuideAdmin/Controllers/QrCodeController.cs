using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TourGuideAdmin.Models;
using TourGuideAdmin.Services;

namespace TourGuideAdmin.Controllers;

[Authorize]
public class QrCodeController : Controller
{
    private readonly ApiService _api;
    public QrCodeController(ApiService api) => _api = api;

    public async Task<IActionResult> Index()
        => View(await _api.GetQrCodesAsync());

    public async Task<IActionResult> Create()
    {
        var pois = await _api.GetPOIsAsync();
        ViewBag.Pois = new SelectList(pois, "Id", "Name");
        return View(new QrCodeViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create(QrCodeViewModel model)
    {
        // Tự sinh QrValue nếu để trống
        if (string.IsNullOrEmpty(model.QrValue))
            model.QrValue = $"POI_{model.PoiId}_{Guid.NewGuid().ToString()[..8].ToUpper()}";

        var ok = await _api.CreateQrCodeAsync(model);
        TempData[ok ? "Success" : "Error"] = ok ? "Tạo QR thành công!" : "Lỗi khi tạo QR.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _api.DeleteQrCodeAsync(id);
        TempData[ok ? "Success" : "Error"] = ok ? "Xóa thành công!" : "Lỗi khi xóa.";
        return RedirectToAction(nameof(Index));
    }
}
