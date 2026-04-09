using Microsoft.EntityFrameworkCore;
using TourGuideAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. THÊM CHÍNH SÁCH CORS (BẮT BUỘC)
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy => {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddDbContext<AudioGuideDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 2. KÍCH HOẠT CORS (Phải đặt trước MapControllers)
app.UseCors("AllowAll");

app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 3. TẠM TẮT HTTPS REDIRECTION (Để test bằng HTTP cho dễ)
// app.UseHttpsRedirection(); 
app.UseStaticFiles();
app.MapControllers();
app.Run();