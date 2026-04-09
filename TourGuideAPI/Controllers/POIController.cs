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

        [HttpGet]
        // ĐÃ THÊM [FromQuery] ĐỂ BẮT CHÍNH XÁC NGÔN NGỮ TỪ APP GỬI LÊN
        public async Task<IActionResult> GetPOIs([FromQuery] string lang = "vi")
        {
            // Chuyển thành chữ thường để tránh lỗi viết hoa/thường (VD: "EN" và "en")
            string langCode = lang.ToLower();

            var data = await _context.Pois
                .Select(p => new
                {
                    Id = p.Id,

                    // CHÌA KHÓA: Lấy Tên Dịch. Nếu không có bản dịch thì giữ lại tên gốc tiếng Anh/Việt.
                    Name = p.Translations
                            .Where(t => t.Language != null && t.Language.Code.ToLower() == langCode)
                            .Select(t => t.Title)
                            .FirstOrDefault() ?? p.Name,

                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    Radius = p.Radius,
                    ImageUrl = p.ImageUrl,

                    // Dự phòng Description gốc để tránh lỗi trắng trang
                    Description = p.Description,

                    // Lấy Nội dung chi tiết và Âm thanh theo đúng ngôn ngữ
                    Content = p.Translations
                        .Where(c => c.Language != null && c.Language.Code.ToLower() == langCode)
                        .Select(c => new
                        {
                            Title = c.Title,
                            Description = c.Content,
                            AudioUrl = p.Audios
                                .Where(a => a.Language != null && a.Language.Code.ToLower() == langCode)
                                .Select(a => a.AudioUrl)
                                .FirstOrDefault()
                        })
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}