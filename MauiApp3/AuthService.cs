using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace MauiApp3;

public class AuthService
{
    private readonly IServiceProvider _serviceProvider;
    private const string CurrentUserIdKey = "current_user_id";
    private const string CurrentUsernameKey = "current_username";

    public AuthService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        System.Diagnostics.Debug.WriteLine("AuthService created");
    }

    public async Task<(bool success, string message, User? user)> RegisterAsync(string username, string password)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"RegisterAsync called for username: {username}");
            
            // Проверка на пустые значения
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return (false, "Логин и пароль не могут быть пустыми", null);
            }

            // Проверка длины
            if (username.Length < 3)
            {
                return (false, "Логин должен быть не менее 3 символов", null);
            }

            if (password.Length < 6)
            {
                return (false, "Пароль должен быть не менее 6 символов", null);
            }

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            // Проверка существования пользователя
            var existingUser = await context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

            if (existingUser != null)
            {
                return (false, "Пользователь с таким логином уже существует", null);
            }

            // Создание нового пользователя
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            var newUser = new User
            {
                Username = username,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(newUser);
            await context.SaveChangesAsync();

            System.Diagnostics.Debug.WriteLine($"User registered successfully: {username} (ID: {newUser.Id})");

            // Сохраняем ID пользователя в Preferences
            Preferences.Set(CurrentUserIdKey, newUser.Id);
            Preferences.Set(CurrentUsernameKey, newUser.Username);

            return (true, "Регистрация успешна!", newUser);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"RegisterAsync error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return (false, $"Ошибка регистрации: {ex.Message}", null);
        }
    }

    public async Task<(bool success, string message, User? user)> LoginAsync(string username, string password)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"LoginAsync called for username: {username}");
            
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return (false, "Логин и пароль не могут быть пустыми", null);
            }

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

            if (user == null)
            {
                System.Diagnostics.Debug.WriteLine($"User not found: {username}");
                return (false, "Неверный логин или пароль", null);
            }

            // Проверка пароля
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

            if (!isPasswordValid)
            {
                System.Diagnostics.Debug.WriteLine($"Invalid password for user: {username}");
                return (false, "Неверный логин или пароль", null);
            }

            // Сохраняем ID пользователя в Preferences
            Preferences.Set(CurrentUserIdKey, user.Id);
            Preferences.Set(CurrentUsernameKey, user.Username);

            System.Diagnostics.Debug.WriteLine($"User logged in successfully: {username} (ID: {user.Id})");

            return (true, "Вход выполнен!", user);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoginAsync error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return (false, $"Ошибка входа: {ex.Message}", null);
        }
    }

    public void Logout()
    {
        Preferences.Remove(CurrentUserIdKey);
        Preferences.Remove(CurrentUsernameKey);
        System.Diagnostics.Debug.WriteLine("User logged out");
    }

    public bool IsUserLoggedIn()
    {
        var isLoggedIn = Preferences.ContainsKey(CurrentUserIdKey);
        System.Diagnostics.Debug.WriteLine($"IsUserLoggedIn: {isLoggedIn}");
        return isLoggedIn;
    }

    public int GetCurrentUserId()
    {
        return Preferences.Get(CurrentUserIdKey, 0);
    }

    public string GetCurrentUsername()
    {
        return Preferences.Get(CurrentUsernameKey, string.Empty);
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return null;

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            return await context.Users.FindAsync(userId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetCurrentUserAsync error: {ex.Message}");
            return null;
        }
    }
}