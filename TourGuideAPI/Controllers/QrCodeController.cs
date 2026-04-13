using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourGuideAPI.Models;

namespace TourGuideAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QrCodeController : ControllerBase
    {
        private readonly AudioGuideDbContext _context;
        public QrCodeController(AudioGuideDbContext context) => _context = context;

        // GET api/QrCode
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.QrCodes
                .Select(q => new
                {
                    q.Id, q.PoiId,
                    PoiName = q.Poi != null ? q.Poi.Name : null,
                    q.QrValue, q.CreatedAt
                })
                .ToListAsync();
            return Ok(data);
        }

        // GET api/QrCode/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var q = await _context.QrCodes
                .Where(q => q.Id == id)
                .Select(q => new
                {
                    q.Id, q.PoiId,
                    PoiName = q.Poi != null ? q.Poi.Name : null,
                    q.QrValue, q.CreatedAt
                })
                .FirstOrDefaultAsync();
            if (q == null) return NotFound();
            return Ok(q);
        }

        // GET api/QrCode/scan?value=xxx  ← Mobile App quét QR
        [HttpGet("scan")]
        public async Task<IActionResult> Scan([FromQuery] string value, [FromQuery] string lang = "vi")
        {
            string langCode = lang.ToLower();
            var qr = await _context.QrCodes
                .Include(q => q.Poi)
                .FirstOrDefaultAsync(q => q.QrValue == value);

            if (qr == null) return NotFound(new { message = "QR không hợp lệ" });

            var poi = await _context.Pois
                .Where(p => p.Id == qr.PoiId)
                .Select(p => new
                {
                    p.Id,
                    Name = p.Translations
                        .Where(t => t.Language != null && t.Language.Code.ToLower() == langCode)
                        .Select(t => t.Title).FirstOrDefault() ?? p.Name,
                    p.Latitude, p.Longitude, p.Radius, p.ImageUrl,
                    Content = p.Translations
                        .Where(t => t.Language != null && t.Language.Code.ToLower() == langCode)
                        .Select(t => new
                        {
                            t.Title, Description = t.Content,
                            AudioUrl = p.Audios
                                .Where(a => a.Language != null && a.Language.Code.ToLower() == langCode)
                                .Select(a => a.AudioUrl).FirstOrDefault()
                        }).FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            return Ok(poi);
        }

        // POST api/QrCode
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] QrCode qr)
        {
            qr.CreatedAt = DateTime.Now;
            _context.QrCodes.Add(qr);
            await _context.SaveChangesAsync();
            return Ok(new { qr.Id, qr.QrValue });
        }

        // DELETE api/QrCode/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var qr = await _context.QrCodes.FindAsync(id);
            if (qr == null) return NotFound();
            _context.QrCodes.Remove(qr);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
