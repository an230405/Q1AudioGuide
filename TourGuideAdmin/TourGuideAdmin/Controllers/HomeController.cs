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

            int onlineCount = await _api.GetActiveUserCountAsync();

            // Mặc định lúc mới vào trang sẽ là "Tất cả thời gian"
            var top5Pois = pois.OrderByDescending(p => p.ListenCount).Take(5).ToList();

            var vm = new DashboardViewModel
            {
                TotalPOIs = pois.Count,
                TotalTranslations = translations.Count,
                TotalLanguages = languages.Count,
                TotalUsers = users.Count,
                TotalAudioLogs = logs.Count,
                ActiveUsersOnline = onlineCount,
                RecentLogs = logs.OrderByDescending(l => l.PlayTime).Take(10).ToList(),
                RecentPOIs = pois.OrderByDescending(p => p.Id).Take(5).ToList(),

                TopPoiNames = top5Pois.Select(p => p.Name).ToList(),
                TopPoiListens = top5Pois.Select(p => p.ListenCount).ToList()
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
        int count = await _api.GetActiveUserCountAsync();
        return Json(new { count = count });
    }

    // ==========================================================
    // 👉 API MỚI: TÍNH TOÁN DỮ LIỆU BIỂU ĐỒ THEO THỜI GIAN
    // ==========================================================
    [HttpGet]
    public async Task<IActionResult> GetChartData(string period)
    {
        try
        {
            List<string> topNames = new List<string>();
            List<int> topCounts = new List<int>();

            // Nếu là "Tất cả", lấy từ cột ListenCount gốc cho chuẩn xác
            if (period == "all")
            {
                var pois = await _api.GetPOIsAsync();
                var top5 = pois.OrderByDescending(p => p.ListenCount).Take(5).ToList();
                topNames = top5.Select(p => p.Name).ToList();
                topCounts = top5.Select(p => p.ListenCount).ToList();
            }
            else
            {
                // Nếu lọc theo ngày, lấy từ nhật ký AudioLog ra đếm
                var logs = await _api.GetAudioLogsAsync();
                var query = logs.AsEnumerable();
                var now = DateTime.Now;

                // Lọc theo mốc thời gian
                if (period == "today")
                    query = query.Where(l => l.PlayTime >= now.Date);
                else if (period == "7days")
                    query = query.Where(l => l.PlayTime >= now.AddDays(-7));
                else if (period == "30days")
                    query = query.Where(l => l.PlayTime >= now.AddDays(-30));

                // Nhóm theo tên địa điểm và đếm số lượt xuất hiện
                var top5Group = query
                    .Where(l => !string.IsNullOrEmpty(l.PoiName))
                    .GroupBy(l => l.PoiName)
                    .Select(g => new { Name = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(5)
                    .ToList();

                topNames = top5Group.Select(x => x.Name).ToList();
                topCounts = top5Group.Select(x => x.Count).ToList();
            }

            return Json(new { success = true, names = topNames, counts = topCounts });
        }
        catch
        {
            return Json(new { success = false });
        }
    }
}