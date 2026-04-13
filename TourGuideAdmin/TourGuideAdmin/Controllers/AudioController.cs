using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TourGuideAdmin.Models;
using TourGuideAdmin.Services;
using Microsoft.AspNetCore.Authorization;

namespace TourGuideAdmin.Controllers;

[Authorize]
public class AudioController : Controller
{
    private readonly ApiService _api;
    private readonly IWebHostEnvironment _env;

    public AudioController(ApiService api, IWebHostEnvironment env)
    {
        _api = api;
        _env = env;
    }

    public async Task<IActionResult> Index()
        => View(await _api.GetAudiosAsync());

    private async Task PopulateDropdowns(int? selectedPoi = null, int? selectedLang = null)
    {
        var pois = await _api.GetPOIsAsync();
        var langs = await _api.GetLanguagesAsync();
        ViewBag.Pois = new SelectList(pois, "Id", "Name", selectedPoi);
        ViewBag.Languages = new SelectList(langs, "Id", "Name", selectedLang);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();
        return View(new AudioViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create(AudioViewModel model, IFormFile? AudioFile)
    {
        // Xử lý upload file nếu có
        if (AudioFile != null && AudioFile.Length > 0)
        {
            var audioDir = Path.Combine(_env.WebRootPath, "audio");
            Directory.CreateDirectory(audioDir);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(AudioFile.FileName)}";
            var filePath = Path.Combine(audioDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await AudioFile.CopyToAsync(stream);

            // Lưu đường dẫn tương đối
            model.AudioUrl = $"/audio/{fileName}";
        }

        if (string.IsNullOrEmpty(model.AudioUrl))
        {
            TempData["Error"] = "Vui lòng chọn file audio hoặc nhập URL.";
            await PopulateDropdowns(model.PoiId, model.LanguageId);
            return View(model);
        }

        var ok = await _api.CreateAudioAsync(model);
        TempData[ok ? "Success" : "Error"] = ok ? "Thêm audio thành công!" : "Lỗi khi thêm.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var audio = await _api.GetAudioAsync(id);
        if (audio == null) return NotFound();
        await PopulateDropdowns(audio.PoiId, audio.LanguageId);
        return View(audio);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, AudioViewModel model, IFormFile? AudioFile)
    {
        // Xử lý upload file mới nếu có
        if (AudioFile != null && AudioFile.Length > 0)
        {
            var audioDir = Path.Combine(_env.WebRootPath, "audio");
            Directory.CreateDirectory(audioDir);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(AudioFile.FileName)}";
            var filePath = Path.Combine(audioDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await AudioFile.CopyToAsync(stream);

            model.AudioUrl = $"/audio/{fileName}";
        }

        var ok = await _api.UpdateAudioAsync(id, model);
        TempData[ok ? "Success" : "Error"] = ok ? "Cập nhật thành công!" : "Lỗi khi cập nhật.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _api.DeleteAudioAsync(id);
        TempData[ok ? "Success" : "Error"] = ok ? "Xóa thành công!" : "Lỗi khi xóa.";
        return RedirectToAction(nameof(Index));
    }
}
