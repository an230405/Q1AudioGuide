using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourGuideAPI.Models;

namespace TourGuideAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AudioLogController : ControllerBase
    {
        private readonly AudioGuideDbContext _context;
        public AudioLogController(AudioGuideDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.AudioLogs
                .OrderByDescending(l => l.PlayTime)
                .Select(l => new
                {
                    l.Id, l.DeviceId, l.PoiId,
                    PoiName = l.Poi != null ? l.Poi.Name : null,
                    l.LanguageId,
                    LanguageName = l.Language != null ? l.Language.Name : null,
                    l.PlayTime
                })
                .ToListAsync();
            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AudioLog log)
        {
            log.PlayTime = DateTime.Now;
            _context.AudioLogs.Add(log);
            await _context.SaveChangesAsync();
            return Ok(new { log.Id });
        }
    }
}
