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

        // 2. LẤY TẤT CẢ CHO APP & ADMIN (api/POI/all?lang=vi)
        // ĐÃ SỬA: Thêm tham số lang và lệnh Select để lấy bản dịch
        [HttpGet("all")]
        public async Task<IActionResult> GetAllPOIs([FromQuery] string lang = "vi")
        {
            return Ok(await ProjectPoiData(lang));
        }

        // 3. LẤY CHI TIẾT KHI QUÉT QR (api/POI/5?lang=vi)
        // ĐÃ SỬA: Trả về dữ liệu có kèm bản dịch Content
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPOI(int id, [FromQuery] string lang = "vi")
        {
            string langCode = lang.ToLower();
            var poi = await ProjectPoiData(langCode, id);

            var result = poi.FirstOrDefault();
            if (result == null) return NotFound();

            return Ok(result);
        }

        // --- HÀM PHỤ TRỢ: Giúp gom nhóm logic lấy dữ liệu tránh viết lặp lại ---
        private async Task<List<object>> ProjectPoiData(string langCode, int? id = null)
        {
            var query = _context.Pois.AsQueryable();
            if (id.HasValue) query = query.Where(p => p.Id == id);

            return await query.Select(p => new
            {
                p.Id,
                // Ưu tiên lấy Title trong bản dịch làm Name
                Name = p.Translations
                    .Where(t => t.Language != null && t.Language.Code.ToLower() == langCode)
                    .Select(t => t.Title)
                    .FirstOrDefault() ?? p.Name,
                p.Latitude,
                p.Longitude,
                p.Radius,
                p.ImageUrl,
                // Lấy nội dung dài (Content) làm Description
                Description = p.Translations
    .Where(t => t.Language != null && t.Language.Code.ToLower() == langCode)
    .Select(t => t.Content) // Lấy ô Content (nội dung dài) của bản dịch
    .FirstOrDefault() ?? p.Description, // Nếu không có bản dịch mới lấy Description gốc
                p.IsActive,
                p.CreatedAt,
                // Giữ nguyên cấu trúc Content cũ để App không bị lỗi
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

        // --- CÁC HÀM TẠO, SỬA, XÓA (GIỮ NGUYÊN CHỨC NĂNG CŨ) ---

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
    }
}