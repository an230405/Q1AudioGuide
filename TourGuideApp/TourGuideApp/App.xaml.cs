namespace TourGuideApp;

public partial class App : Application
{
    // 1. Giữ lại biến cũ để dập tắt lỗi ở các file cũ (MapPage, QRScanner...)
    public static int CurrentLanguage { get; set; } = 0;

    // 2. SỬA DÒNG NÀY: Dùng get; set; để App lưu mã ngôn ngữ từ MainPage vào được
    public static string CurrentLanguageCode { get; set; } = "vi";

    public App()
    {
        InitializeComponent();
        MainPage = new AppShell(); // Hoặc NavigationPage tùy code cũ của Anh
    }
}