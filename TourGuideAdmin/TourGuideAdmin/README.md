# TourGuideAdmin — Web Admin Panel

Web Admin cho app Audio Guide Tour Q1 TP.HCM.  
Công nghệ: **ASP.NET Core 8 MVC**, gọi API qua HTTP.

## Cấu trúc project

```
TourGuideAdmin/
├── Controllers/
│   ├── HomeController.cs       ← Dashboard
│   ├── POIController.cs        ← Địa điểm
│   ├── AudioController.cs      ← Audio
│   ├── TranslationController.cs← Bản dịch
│   ├── LanguageController.cs   ← Ngôn ngữ
│   ├── UserController.cs       ← Người dùng
│   └── AudioLogController.cs   ← Nhật ký phát
├── Models/
│   └── ViewModels.cs           ← Tất cả ViewModel
├── Services/
│   └── ApiService.cs           ← Gọi HTTP đến TourGuideAPI
├── Views/                      ← Razor Views (Index, Create, Edit)
├── appsettings.json
└── Program.cs
```

## Cách chạy

### 1. Cấu hình URL API
Mở `appsettings.json`, sửa `ApiBaseUrl` thành địa chỉ TourGuideAPI của bạn:
```json
{
  "ApiBaseUrl": "http://localhost:5159/"
}
```

### 2. Bật TourGuideAPI trước
Chạy project `TourGuideAPI` trong Visual Studio (hoặc `dotnet run`).

### 3. Chạy TourGuideAdmin
```
dotnet run
```
Truy cập: `http://localhost:5000`

## Lưu ý về API

Web Admin gọi các endpoint sau trên TourGuideAPI.  
**Nếu API chưa có các endpoint CRUD đầy đủ**, bạn cần bổ sung vào `TourGuideAPI`:

| Controller     | Endpoints cần có                              |
|---------------|-----------------------------------------------|
| POIController  | GET /api/POI, GET /api/POI/{id}, POST, PUT, DELETE |
| AudioController| GET /api/Audio, GET /api/Audio/{id}, POST, PUT, DELETE |
| TranslationController | tương tự |
| LanguageController | tương tự |
| UserController | tương tự |
| AudioLogController | GET /api/AudioLog |

## Tính năng

- ✅ Dashboard tổng quan (số liệu thống kê)
- ✅ Quản lý địa điểm (POI): thêm, sửa, xóa, bật/tắt hiển thị
- ✅ Quản lý Audio: thêm, sửa, xóa
- ✅ Quản lý Bản dịch (4 ngôn ngữ: VI, EN, JA, ZH)
- ✅ Quản lý Ngôn ngữ
- ✅ Quản lý Người dùng
- ✅ Xem Nhật ký phát audio (AudioLog)
