using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourGuideAPI.Models;

[Route("api/[controller]")]
[ApiController]
public class POIController : ControllerBase
{
    private readonly AudioGuideDbContext _context;

    public POIController(AudioGuideDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetPOIs(string lang = "vi")
    {
        var data = await _context.Pois
            .Include(p => p.Translations)
            .Include(p => p.Audios) // Đừng quên cái này nhé
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Latitude,
                p.Longitude,
                p.Radius,
                p.ImageUrl,
                // Sửa chỗ này để bọc Title, Description và AudioUrl vào chung object Content
                Content = p.Translations
                    .Where(c => c.Language.Code == lang)
                    .Select(c => new
                    {
                        c.Title,
                        Description = c.Content,
                        // Đưa AudioUrl vào đây cho khớp với Model phía App
                        AudioUrl = p.Audios
                            .Where(a => a.Language.Code == lang)
                            .Select(a => a.AudioUrl)
                            .FirstOrDefault()
                    })
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(data);
    }
}