using Microsoft.EntityFrameworkCore;

namespace MauiApp3;

public partial class App : Application
{
    public App(IServiceProvider serviceProvider)
    {
        try
        {
            InitializeComponent();

            System.Diagnostics.Debug.WriteLine("=== App Starting ===");

            // Сначала устанавливаем MainPage
            MainPage = new AppShell();

            System.Diagnostics.Debug.WriteLine("AppShell created");

            // Инициализация базы данных
            InitializeDatabase(serviceProvider);

            System.Diagnostics.Debug.WriteLine("Database initialized");

            // ВАЖНО: Навигация должна происходить ПОСЛЕ того как MainPage установлена
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await Task.Delay(100); // Даем время на инициализацию Shell
                    
                    var authService = serviceProvider.GetRequiredService<AuthService>();
                    
                    if (authService.IsUserLoggedIn())
                    {
                        System.Diagnostics.Debug.WriteLine("User is logged in, navigating to MainPage");
                        await Shell.Current.GoToAsync("//MainPage");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("User is not logged in, navigating to LoginPage");
                        await Shell.Current.GoToAsync("//LoginPage");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== APP INITIALIZATION ERROR ===");
            System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
        }
    }

    private void InitializeDatabase(IServiceProvider serviceProvider)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Starting database initialization...");
            
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            
            System.Diagnostics.Debug.WriteLine("DatabaseContext retrieved");
            
            // Применяем миграции автоматически
            context.Database.EnsureCreated(); // Для SQLite - создает БД если её нет
            
            System.Diagnostics.Debug.WriteLine("Database created/ensured");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== DATABASE INITIALIZATION ERROR ===");
            System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
        }
    }
}