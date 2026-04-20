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
        // 👉 ĐÃ SỬA: Thêm onlyActive: true để App chỉ tải những địa điểm đang "Hiện"
        [HttpGet]
        public async Task<IActionResult> GetPOIs([FromQuery] string lang = "vi")
        {
            return Ok(await ProjectPoiData(lang, id: null, onlyActive: true));
        }

        // 2. LẤY TẤT CẢ CHO ADMIN (api/POI/all?lang=vi)
        // 👉 ĐÃ SỬA: onlyActive: false để Admin thấy được toàn bộ để quản lý
        [HttpGet("all")]
        public async Task<IActionResult> GetAllPOIs([FromQuery] string lang = "vi")
        {
            return Ok(await ProjectPoiData(lang, id: null, onlyActive: false));
        }

        // 3. LẤY CHI TIẾT KHI QUÉT QR (api/POI/5?lang=vi)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPOI(int id, [FromQuery] string lang = "vi")
        {
            string langCode = lang.ToLower();
            var poiList = await ProjectPoiData(langCode, id, onlyActive: false);
            var result = poiList.FirstOrDefault();

            if (result == null) return NotFound();
            return Ok(result);
        }

        // --- HÀM PHỤ TRỢ: CHỐNG LẶP CODE ---
        // 👉 ĐÃ SỬA: Thêm tham số bool onlyActive = false vào hàm
        private async Task<List<object>> ProjectPoiData(string langCode, int? id = null, bool onlyActive = false)
        {
            var query = _context.Pois.AsQueryable();

            if (id.HasValue) query = query.Where(p => p.Id == id);

            // 👉 BỘ LỌC ẨN/HIỆN NẰM Ở ĐÂY:
            if (onlyActive)
            {
                query = query.Where(p => p.IsActive == true);
            }

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

            try
            {
                // 1. Dọn dẹp Bản dịch (Translations)
                var translations = _context.Translations.Where(t => t.PoiId == id);
                _context.Translations.RemoveRange(translations);

                // 2. Dọn dẹp Audio
                var audios = _context.Audios.Where(a => a.PoiId == id);
                _context.Audios.RemoveRange(audios);

                // 3. Dọn dẹp Nhật ký nghe (AudioLogs)
                var logs = _context.AudioLogs.Where(l => l.PoiId == id);
                _context.AudioLogs.RemoveRange(logs);

                // 4. Dọn dẹp QR Code (nếu có bảng này)
                // Nếu project Anh không có bảng QrCodes thì Anh cứ mạnh dạn xóa 2 dòng này đi nhé
                var qrs = _context.QrCodes.Where(q => q.PoiId == id);
                _context.QrCodes.RemoveRange(qrs);

                // 5. Cuối cùng: Xóa thẳng tay Địa điểm
                _context.Pois.Remove(poi);

                // Lưu thay đổi 1 lần duy nhất cho tối ưu
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                // Bắt lỗi in ra cửa sổ Output để lỡ có trục trặc mình còn biết đường sửa
                System.Diagnostics.Debug.WriteLine($"Lỗi xóa DB: {ex.Message}");
                return BadRequest("Không thể xóa địa điểm này do vướng dữ liệu liên quan.");
            }
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

            // 2. THÊM MỚI: Chỉ lưu ID và Thời gian vào nhật ký
            var log = new AudioLog
            {
                PoiId = poi.Id,
                PlayTime = DateTime.Now
            };
            _context.AudioLogs.Add(log);

            // 3. Lưu tất cả lại
            await _context.SaveChangesAsync();

            return Ok(new { currentListens = poi.ListenCount });
        }
    }
}