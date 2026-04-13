using Microsoft.AspNetCore.Mvc;
using TourGuideAdmin.Services;
using Microsoft.AspNetCore.Authorization;

namespace TourGuideAdmin.Controllers;

[Authorize]
public class AudioLogController : Controller
{
    private readonly ApiService _api;
    public AudioLogController(ApiService api) => _api = api;

    public async Task<IActionResult> Index() => View(await _api.GetAudioLogsAsync());
}
