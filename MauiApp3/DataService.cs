using System.Collections.ObjectModel;
using System.Text.Json;

namespace MauiApp3;

public static class DataService
{
    private static readonly string customListsKey = "custom_lists";
    private static readonly string itemsPrefix = "items_";

    public static ObservableCollection<ThingsList> GetCustomLists()
    {
        var json = Preferences.Get(customListsKey, string.Empty);
        if (string.IsNullOrEmpty(json))
        {
            return new ObservableCollection<ThingsList>();
        }

        try
        {
            var lists = JsonSerializer.Deserialize<List<ThingsList>>(json);
            return new ObservableCollection<ThingsList>(lists ?? new List<ThingsList>());
        }
        catch
        {
            return new ObservableCollection<ThingsList>();
        }
    }

    public static ThingsList CreateCustomList(string name)
    {
        var lists = GetCustomLists();
        var newList = new ThingsList
        {
            Id = Guid.NewGuid().ToString(),
            Name = name
        };

        lists.Add(newList);
        SaveCustomLists(lists);
        return newList;
    }

    private static void SaveCustomLists(ObservableCollection<ThingsList> lists)
    {
        var json = JsonSerializer.Serialize(lists.ToList());
        Preferences.Set(customListsKey, json);
    }

    public static ObservableCollection<ThingItem> GetItemsForList(string listName, string listId = null)
    {
        // Определяем ключ для хранения
        string key = string.IsNullOrEmpty(listId) 
            ? $"{itemsPrefix}{listName}" 
            : $"{itemsPrefix}{listId}";

        var json = Preferences.Get(key, string.Empty);

        // Если данных нет, возвращаем предустановленные элементы
        if (string.IsNullOrEmpty(json))
        {
            return GetDefaultItems(listName);
        }

        try
        {
            var items = JsonSerializer.Deserialize<List<ThingItem>>(json);
            return new ObservableCollection<ThingItem>(items ?? new List<ThingItem>());
        }
        catch
        {
            return GetDefaultItems(listName);
        }
    }

    private static ObservableCollection<ThingItem> GetDefaultItems(string listName)
    {
        var items = new ObservableCollection<ThingItem>();

        switch (listName)
        {
            case "Перед выходом из дома":
                items.Add(new ThingItem { Id = Guid.NewGuid().ToString(), Name = "Ключи", IsChecked = false, TextDecoration = TextDecorations.None });
                items.Add(new ThingItem { Id = Guid.NewGuid().ToString(), Name = "Кошелек", IsChecked = false, TextDecoration = TextDecorations.None });
                items.Add(new ThingItem { Id = Guid.NewGuid().ToString(), Name = "Телефон", IsChecked = false, TextDecoration = TextDecorations.None });
                items.Add(new ThingItem { Id = Guid.NewGuid().ToString(), Name = "Документы", IsChecked = false, TextDecoration = TextDecorations.None });
                break;

            case "В поездку":
                items.Add(new ThingItem { Id = Guid.NewGuid().ToString(), Name = "Паспорт", IsChecked = false, TextDecoration = TextDecorations.None });
                items.Add(new ThingItem { Id = Guid.NewGuid().ToString(), Name = "Зарядка", IsChecked = false, TextDecoration = TextDecorations.None });
                items.Add(new ThingItem { Id = Guid.NewGuid().ToString(), Name = "Наушники", IsChecked = false, TextDecoration = TextDecorations.None });
                items.Add(new ThingItem { Id = Guid.NewGuid().ToString(), Name = "Билеты", IsChecked = false, TextDecoration = TextDecorations.None });
                items.Add(new ThingItem { Id = Guid.NewGuid().ToString(), Name = "Одежда", IsChecked = false, TextDecoration = TextDecorations.None });
                break;

            case "На природу":
                items.Add(new ThingItem { Id = Guid.NewGuid().ToString(), Name = "Коврик", IsChecked = false, TextDecoration = TextDecorations.None });
                items.Add(new ThingItem { Id = Guid.NewGuid().ToString(), Name = "Вода", IsChecked = false, TextDecoration = TextDecorations.None });
                items.Add(new ThingItem { Id = Guid.NewGuid().ToString(), Name = "Еда", IsChecked = false, TextDecoration = TextDecorations.None });
                items.Add(new ThingItem { Id = Guid.NewGuid().ToString(), Name = "Средство от комаров", IsChecked = false, TextDecoration = TextDecorations.None });
                items.Add(new ThingItem { Id = Guid.NewGuid().ToString(), Name = "Фонарик", IsChecked = false, TextDecoration = TextDecorations.None });
                break;
        }

        return items;
    }

    public static void SaveItems(string listName, string listId, ObservableCollection<ThingItem> items)
    {
        string key = string.IsNullOrEmpty(listId) 
            ? $"{itemsPrefix}{listName}" 
            : $"{itemsPrefix}{listId}";

        var json = JsonSerializer.Serialize(items.ToList());
        Preferences.Set(key, json);
    }
}