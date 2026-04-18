using Microsoft.AspNetCore.Authentication.Cookies;
using TourGuideAdmin.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// 1. Cấu hình HTTP Client để gọi API
builder.Services.AddHttpClient("TourGuideAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://f8lzzzn0-7182.asse.devtunnels.ms/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddScoped<ApiService>();

// 2. Cấu hình Phân quyền (DUY NHẤT 1 LẦN Ở ĐÂY)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

// ==========================================
var app = builder.Build();
// ==========================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();

// 3. Bật bảo vệ (Bắt buộc phải theo thứ tự này)
app.UseAuthentication();
app.UseAuthorization();

// 4. Trang mặc định khi mở Web là trang Đăng nhập
// Đổi chữ Account thành Home, chữ Login thành Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();