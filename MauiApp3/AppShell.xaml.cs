namespace MauiApp3;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        // Регистрация маршрутов для навигации
        Routing.RegisterRoute("RegisterPage", typeof(RegisterPage));
        Routing.RegisterRoute("ItemsPage", typeof(ItemsPage)); // если есть
    }
}