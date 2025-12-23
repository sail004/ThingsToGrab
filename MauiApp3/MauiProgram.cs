
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MauiApp3;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif
        
        var connectionString = "Host=10.163.1.136;Port=5432;Database=postgres;Username=postgres;Password=password;Timeout=30;CommandTimeout=30;";

        // Регистрация DatabaseContext
        builder.Services.AddDbContext<DatabaseContext>(options =>
            options.UseNpgsql(connectionString));

        // Регистрация сервисов
        builder.Services.AddSingleton<AuthService>();

        // Регистрация страниц
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<MainPage>();
        // Добавьте другие страницы, если нужно

        return builder.Build();
    }
}