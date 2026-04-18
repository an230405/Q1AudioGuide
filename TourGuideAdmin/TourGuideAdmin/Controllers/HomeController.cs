using Microsoft.AspNetCore.Mvc;
using TourGuideAdmin.Models;
using TourGuideAdmin.Services;
using Microsoft.AspNetCore.Authorization;

namespace TourGuideAdmin.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ApiService _api;
    public HomeController(ApiService api) => _api = api;

    public async Task<IActionResult> Index()
    {
        try
        {
            var pois = await _api.GetPOIsAsync();
            var translations = await _api.GetTranslationsAsync();
            var languages = await _api.GetLanguagesAsync();
            var users = await _api.GetUsersAsync();
            var logs = await _api.GetAudioLogsAsync();

            // Lấy số người đang Online hiện tại!
            int onlineCount = await _api.GetActiveUserCountAsync();

            var vm = new DashboardViewModel
            {
                TotalPOIs = pois.Count,
                TotalTranslations = translations.Count,
                TotalLanguages = languages.Count,
                TotalUsers = users.Count,
                TotalAudioLogs = logs.Count,
                ActiveUsersOnline = onlineCount, // 👉 Đẩy số ra giao diện
                RecentLogs = logs.OrderByDescending(l => l.PlayTime).Take(10).ToList(),
                RecentPOIs = pois.Take(5).ToList()
            };
            return View(vm);
        }
        catch
        {
            return View(new DashboardViewModel());
        }
    }
    [HttpGet]
    public async Task<IActionResult> GetOnlineCount()
    {
        // Gọi sang Web API để lấy con số mới nhất
        int count = await _api.GetActiveUserCountAsync();
        return Json(new { count = count });
    }
}
