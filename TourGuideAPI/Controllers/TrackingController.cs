using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace TourGuideAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrackingController : ControllerBase
    {
        // Bộ nhớ siêu tốc lưu danh sách các điện thoại đang mở App
        private static ConcurrentDictionary<string, DateTime> _activeDevices = new();

        // 1. APP MOBILE SẼ GỌI HÀM NÀY MỖI 10 GIÂY (Đập nhịp tim)
        [HttpPost("ping")]
        public IActionResult Ping([FromQuery] string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId)) return BadRequest();

            // Cập nhật lại đồng hồ: "Thiết bị này vừa lên tiếng lúc mấy giờ"
            _activeDevices[deviceId] = DateTime.Now;

            return Ok(new { message = "Pong" });
        }

        // 2. TRANG ADMIN SẼ GỌI HÀM NÀY ĐỂ XEM SỐ NGƯỜI ĐANG ONLINE
        [HttpGet("active-count")]
        public IActionResult GetActiveCount()
        {
            // Mốc thời gian: Lấy hiện tại lùi về trước 20 giây
            var cutoffTime = DateTime.Now.AddSeconds(-20);

            // Tìm những điện thoại đã "nín thở" quá 20 giây (tắt App hoặc rớt mạng)
            var offlineDevices = _activeDevices
                .Where(kvp => kvp.Value < cutoffTime)
                .Select(kvp => kvp.Key)
                .ToList();

            // Xóa tụi nó khỏi danh sách Online
            foreach (var id in offlineDevices)
            {
                _activeDevices.TryRemove(id, out _);
            }

            // Trả về con số cuối cùng cho Admin
            return Ok(new
            {
                onlineUsers = _activeDevices.Count
            });
        }
    }
}