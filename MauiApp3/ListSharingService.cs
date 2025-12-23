using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace MauiApp3;

public class ListSharingService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly AuthService _authService;

    public ListSharingService(IServiceProvider serviceProvider, AuthService authService)
    {
        _serviceProvider = serviceProvider;
        _authService = authService;
    }

    // Поделиться списком с другим пользователем
    public async Task<(bool success, string message, int? sharedListId)> ShareListAsync(
        string listId, 
        string listName, 
        IEnumerable<ThingItem> items, 
        string recipientUsername)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"ShareListAsync: {listName} to {recipientUsername}");

            if (string.IsNullOrWhiteSpace(recipientUsername))
            {
                return (false, "Введите имя пользователя", null);
            }

            var currentUserId = _authService.GetCurrentUserId();
            if (currentUserId == 0)
            {
                return (false, "Вы не авторизованы", null);
            }

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            // Проверяем, существует ли получатель
            var recipient = await context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == recipientUsername.ToLower());

            if (recipient == null)
            {
                return (false, $"Пользователь '{recipientUsername}' не найден", null);
            }

            if (recipient.Id == currentUserId)
            {
                return (false, "Нельзя поделиться списком с самим собой", null);
            }

            // Сериализуем items в JSON
            var itemsJson = JsonSerializer.Serialize(items);

            // Создаем запись в shared_lists
            var sharedList = new SharedList
            {
                ListId = listId ?? Guid.NewGuid().ToString(),
                ListName = listName,
                OwnerId = currentUserId,
                ListData = itemsJson,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.SharedLists.Add(sharedList);
            await context.SaveChangesAsync();

            System.Diagnostics.Debug.WriteLine($"SharedList created with ID: {sharedList.Id}");

            // Создаем доступ для получателя
            var access = new SharedListAccess
            {
                ListId = sharedList.Id,
                UserId = recipient.Id,
                AccessLevel = "view",
                CreatedAt = DateTime.UtcNow
            };

            context.SharedListAccesses.Add(access);
            await context.SaveChangesAsync();

            System.Diagnostics.Debug.WriteLine($"Access granted to user: {recipient.Username}");

            return (true, $"Список успешно расшарен!\nID списка: {sharedList.Id}", sharedList.Id);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ShareListAsync error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return (false, $"Ошибка: {ex.Message}", null);
        }
    }

    // Получить расшаренный список по ID
    public async Task<(bool success, string message, SharedList? sharedList)> GetSharedListAsync(int sharedListId)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"GetSharedListAsync: ID {sharedListId}");

            var currentUserId = _authService.GetCurrentUserId();
            var currentUsername = _authService.GetCurrentUsername();

            if (currentUserId == 0)
            {
                return (false, "Вы не авторизованы", null);
            }

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            // Загружаем список с информацией о владельце
            var sharedList = await context.SharedLists
                .Include(sl => sl.Owner)
                .Include(sl => sl.SharedAccesses)
                .FirstOrDefaultAsync(sl => sl.Id == sharedListId);

            if (sharedList == null)
            {
                return (false, "Список с таким ID не найден", null);
            }

            System.Diagnostics.Debug.WriteLine($"Found list: {sharedList.ListName}, Owner: {sharedList.Owner.Username}");

            // Проверяем права доступа
            var hasAccess = sharedList.SharedAccesses.Any(sa => sa.UserId == currentUserId) 
                         || sharedList.OwnerId == currentUserId;

            if (!hasAccess)
            {
                return (false, $"У вас нет доступа к этому списку.\nСписок расшарен не для пользователя '{currentUsername}'", null);
            }

            return (true, "Список найден!", sharedList);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetSharedListAsync error: {ex.Message}");
            return (false, $"Ошибка: {ex.Message}", null);
        }
    }

    // Загрузить расшаренный список в локальное хранилище
    public (bool success, string message, ThingsList? localList) LoadSharedListToLocal(SharedList sharedList)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"LoadSharedListToLocal: {sharedList.ListName}");

            // Десериализуем items
            var items = JsonSerializer.Deserialize<List<ThingItem>>(sharedList.ListData);
            
            if (items == null || !items.Any())
            {
                return (false, "Список пуст", null);
            }

            // Создаем локальную копию списка
            var localList = DataService.CreateCustomList($"{sharedList.ListName} (общий #{sharedList.Id})");

            // Сохраняем items
            DataService.SaveItems(localList.Name, localList.Id, new System.Collections.ObjectModel.ObservableCollection<ThingItem>(items));

            System.Diagnostics.Debug.WriteLine($"List loaded to local storage: {localList.Name}");

            return (true, "Список успешно добавлен!", localList);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadSharedListToLocal error: {ex.Message}");
            return (false, $"Ошибка загрузки: {ex.Message}", null);
        }
    }

    // Обновить расшаренный список (для владельца)
    public async Task<(bool success, string message)> UpdateSharedListAsync(
        int sharedListId,
        IEnumerable<ThingItem> items)
    {
        try
        {
            var currentUserId = _authService.GetCurrentUserId();
            if (currentUserId == 0)
            {
                return (false, "Вы не авторизованы");
            }

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            var sharedList = await context.SharedLists.FindAsync(sharedListId);

            if (sharedList == null)
            {
                return (false, "Список не найден");
            }

            if (sharedList.OwnerId != currentUserId)
            {
                return (false, "Только владелец может обновлять список");
            }

            // Обновляем данные
            sharedList.ListData = JsonSerializer.Serialize(items);
            sharedList.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return (true, "Список обновлен!");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateSharedListAsync error: {ex.Message}");
            return (false, $"Ошибка обновления: {ex.Message}");
        }
    }
}