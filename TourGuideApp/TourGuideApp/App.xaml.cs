namespace TourGuideApp;

public partial class App : Application
{
    // THÊM DÒNG NÀY ĐỂ LƯU NGÔN NGỮ TOÀN APP
    public static int CurrentLanguage = 0;

    public App()
    {
        InitializeComponent();
        MainPage = new NavigationPage(new MainPage());
    }
}