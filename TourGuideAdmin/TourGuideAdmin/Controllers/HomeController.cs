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
            var audios = await _api.GetAudiosAsync();
            var translations = await _api.GetTranslationsAsync();
            var languages = await _api.GetLanguagesAsync();
            var users = await _api.GetUsersAsync();
            var logs = await _api.GetAudioLogsAsync();

            var vm = new DashboardViewModel
            {
                TotalPOIs = pois.Count,
                TotalAudios = audios.Count,
                TotalTranslations = translations.Count,
                TotalLanguages = languages.Count,
                TotalUsers = users.Count,
                TotalAudioLogs = logs.Count,
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
}
