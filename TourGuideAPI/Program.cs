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

app.UseCors("AllowAll");

// CHỈ CẦN 1 DÒNG NÀY ĐỂ MỞ CỬA CHO ẢNH
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // Tạm tắt để test cho dễ như Anh đã làm

app.MapControllers();
app.Run();