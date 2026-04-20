using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourGuideAPI.Models;

namespace TourGuideAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class POIController : ControllerBase
    {
        private readonly AudioGuideDbContext _context;

        public POIController(AudioGuideDbContext context)
        {
            _context = context;
        }

        // 1. LẤY DANH SÁCH CHO APP (api/POI?lang=vi)
        [HttpGet]
        public async Task<IActionResult> GetPOIs([FromQuery] string lang = "vi")
        {
            return Ok(await ProjectPoiData(lang));
        }

        // 2. LẤY TẤT CẢ CHO ADMIN & APP (api/POI/all?lang=vi)
        [HttpGet("all")]
        public async Task<IActionResult> GetAllPOIs([FromQuery] string lang = "vi")
        {
            return Ok(await ProjectPoiData(lang));
        }

        // 3. LẤY CHI TIẾT KHI QUÉT QR (api/POI/5?lang=vi)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPOI(int id, [FromQuery] string lang = "vi")
        {
            string langCode = lang.ToLower();
            var poiList = await ProjectPoiData(langCode, id);
            var result = poiList.FirstOrDefault();

            if (result == null) return NotFound();
            return Ok(result);
        }

        // --- HÀM PHỤ TRỢ: CHỐNG LẶP CODE ---
        private async Task<List<object>> ProjectPoiData(string langCode, int? id = null)
        {
            var query = _context.Pois.AsQueryable();
            if (id.HasValue) query = query.Where(p => p.Id == id);

            return await query.Select(p => new
            {
                p.Id,
                Name = p.Translations
                    .Where(t => t.Language != null && t.Language.Code.ToLower() == langCode)
                    .Select(t => t.Title)
                    .FirstOrDefault() ?? p.Name,
                p.Latitude,
                p.Longitude,
                p.Radius,
                p.ImageUrl,
                Description = p.Translations
                    .Where(t => t.Language != null && t.Language.Code.ToLower() == langCode)
                    .Select(t => t.Content)
                    .FirstOrDefault() ?? p.Description,
                p.IsActive,
                p.CreatedAt,
                p.ViewCount,
                p.ListenCount,
                p.PriorityScore,
                Content = p.Translations
                    .Where(c => c.Language != null && c.Language.Code.ToLower() == langCode)
                    .Select(c => new
                    {
                        c.Title,
                        Description = c.Content,
                        AudioUrl = p.Audios
                            .Where(a => a.Language != null && a.Language.Code.ToLower() == langCode)
                            .Select(a => a.AudioUrl)
                            .FirstOrDefault()
                    })
                    .FirstOrDefault()
            })
            .Cast<object>()
            .ToListAsync();
        }

        [HttpPost]
        public async Task<IActionResult> CreatePOI([FromBody] Poi poi)
        {
            poi.CreatedAt = DateTime.Now;
            _context.Pois.Add(poi);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPOI), new { id = poi.Id }, poi);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePOI(int id, [FromBody] Poi poi)
        {
            if (id != poi.Id) return BadRequest();

            _context.Entry(poi).State = EntityState.Modified;
            _context.Entry(poi).Property(x => x.CreatedAt).IsModified = false;

            // Khóa không cho sửa số lượt xem/nghe thủ công qua form
            _context.Entry(poi).Property(x => x.ViewCount).IsModified = false;
            _context.Entry(poi).Property(x => x.ListenCount).IsModified = false;

            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Pois.Any(e => e.Id == id)) return NotFound();
                throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePOI(int id)
        {
            var poi = await _context.Pois.FindAsync(id);
            if (poi == null) return NotFound();
            _context.Pois.Remove(poi);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id}/increase-view")]
        public async Task<IActionResult> IncreaseView(int id)
        {
            var poi = await _context.Pois.FindAsync(id);
            if (poi == null) return NotFound();
            poi.ViewCount++;
            await _context.SaveChangesAsync();
            return Ok(new { currentViews = poi.ViewCount });
        }

        [HttpPost("{id}/increase-listen")]
        public async Task<IActionResult> IncreaseListen(int id)
        {
            var poi = await _context.Pois.FindAsync(id);
            if (poi == null) return NotFound();

            // 1. Tăng tổng số
            poi.ListenCount++;

            // 👉 2. THÊM MỚI: Chỉ lưu ID và Thời gian vào nhật ký
            var log = new AudioLog
            {
                PoiId = poi.Id,
                // Đã xóa dòng PoiName vì Database không cần
                PlayTime = DateTime.Now
            };
            _context.AudioLogs.Add(log);

            // 3. Lưu tất cả lại
            await _context.SaveChangesAsync();

            return Ok(new { currentListens = poi.ListenCount });
        }
    }
}