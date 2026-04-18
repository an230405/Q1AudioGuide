using Microsoft.AspNetCore.Mvc;
using TourGuideAdmin.Models;
using TourGuideAdmin.Services;
using Microsoft.AspNetCore.Authorization;

namespace TourGuideAdmin.Controllers;

// 👉 KHÓA TOÀN BỘ CLASS: Cấm cửa tuyệt đối ai không phải là Admin
[Authorize(Roles = "admin,Admin")]
public class UserController : Controller
{
    private readonly ApiService _api;
    public UserController(ApiService api) => _api = api;

    public async Task<IActionResult> Index() => View(await _api.GetUsersAsync());
    public IActionResult Create() => View(new UserViewModel { Role = "admin" });

    [HttpPost]
    public async Task<IActionResult> Create(UserViewModel model)
    {
        var ok = await _api.CreateUserAsync(model);
        TempData[ok ? "Success" : "Error"] = ok ? "Thêm người dùng thành công!" : "Lỗi khi thêm.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var u = await _api.GetUserAsync(id);
        if (u == null) return NotFound();
        return View(u);
    }

    [Authorize(Roles = "admin,Admin")]
    [HttpPost]
    public async Task<IActionResult> Edit(int id, UserViewModel model)
    {
        // Nếu Anh có dùng mã hóa BCrypt như em hướng dẫn lúc nãy, 
        // thì nhớ kiểm tra: nếu Password không rỗng thì mới băm rồi gửi đi.

        var ok = await _api.UpdateUserAsync(id, model);

        if (ok)
        {
            TempData["Success"] = "Cập nhật thông tin người dùng thành công!";
        }
        else
        {
            TempData["Error"] = "Có lỗi xảy ra khi cập nhật người dùng.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _api.DeleteUserAsync(id);
        TempData[ok ? "Success" : "Error"] = ok ? "Xóa thành công!" : "Lỗi khi xóa.";
        return RedirectToAction(nameof(Index));
    }
}