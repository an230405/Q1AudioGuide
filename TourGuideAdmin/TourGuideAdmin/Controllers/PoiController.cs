using Microsoft.AspNetCore.Mvc;
using TourGuideAdmin.Models;

public class POIController : Controller
{
    private readonly HttpClient _http;

    public POIController()
    {
        _http = new HttpClient();
        _http.BaseAddress = new Uri("https://gwsmx4vm-7182.asse.devtunnels.ms/swagger/index.html");
    }

    // LIST
    public async Task<IActionResult> Index()
    {
        var data = await _http.GetFromJsonAsync<List<POI>>("api/poi");
        return View(data);
    }

    // CREATE GET
    public IActionResult Create()
    {
        return View();
    }

    // CREATE POST
    [HttpPost]
    public async Task<IActionResult> Create(POI poi)
    {
        await _http.PostAsJsonAsync("api/poi", poi);
        return RedirectToAction("Index");
    }

    // DELETE
    public async Task<IActionResult> Delete(int id)
    {
        await _http.DeleteAsync($"api/poi/{id}");
        return RedirectToAction("Index");
    }
}