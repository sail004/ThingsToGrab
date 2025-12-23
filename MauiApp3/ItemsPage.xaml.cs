using System.Collections.ObjectModel;

namespace MauiApp3;

public partial class ItemsPage : ContentPage
{
    private string listName;
    private string listId;
    private ObservableCollection<ThingItem> items;
    private readonly ListSharingService _sharingService;

    public ItemsPage(string name, string id = null)
    {
        InitializeComponent();
        
        // Получаем сервис через DI
        _sharingService = Application.Current.Handler.MauiContext.Services.GetService<ListSharingService>();
        
        listName = name;
        listId = id;
        TitleLabel.Text = listName;
        LoadItems();
    }

    private void LoadItems()
    {
        items = DataService.GetItemsForList(listName, listId);
        ItemsCollection.ItemsSource = items;
    }

    private void OnCheckBoxChanged(object sender, CheckedChangedEventArgs e)
    {
        var checkBox = sender as CheckBox;
        var item = checkBox?.BindingContext as ThingItem;
        
        if (item != null)
        {
            item.IsChecked = e.Value;
            item.TextDecoration = e.Value ? TextDecorations.Strikethrough : TextDecorations.None;
            DataService.SaveItems(listName, listId, items);
        }
    }

    private async void OnAddItemClicked(object sender, EventArgs e)
    {
        string result = await DisplayPromptAsync(
            "Добавить вещь", 
            "Введите название:",
            "Добавить",
            "Отмена",
            placeholder: "Название вещи");

        if (!string.IsNullOrWhiteSpace(result))
        {
            var newItem = new ThingItem 
            { 
                Id = Guid.NewGuid().ToString(),
                Name = result, 
                IsChecked = false,
                TextDecoration = TextDecorations.None
            };
            items.Add(newItem);
            DataService.SaveItems(listName, listId, items);
        }
    }

    private async void OnShareListClicked(object sender, EventArgs e)
    {
        if (_sharingService == null)
        {
            await DisplayAlert("Ошибка", "Сервис расшаривания недоступен", "OK");
            return;
        }

        // Проверяем, есть ли items в списке
        if (items == null || items.Count == 0)
        {
            await DisplayAlert("Внимание", "Список пуст. Добавьте хотя бы одну вещь перед расшариванием.", "OK");
            return;
        }

        // Запрашиваем username получателя
        string recipientUsername = await DisplayPromptAsync(
            "Поделиться списком",
            "Введите имя пользователя:",
            "Поделиться",
            "Отмена",
            placeholder: "username");

        if (string.IsNullOrWhiteSpace(recipientUsername))
        {
            return;
        }

        // Показываем индикатор загрузки
        var loadingPage = new ContentPage
        {
            Content = new ActivityIndicator
            {
                IsRunning = true,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Color = Colors.Gray
            }
        };

        await Navigation.PushModalAsync(loadingPage);

        try
        {
            // Расшариваем список
            var result = await _sharingService.ShareListAsync(
                listId,
                listName,
                items,
                recipientUsername);

            await Navigation.PopModalAsync();

            if (result.success)
            {
                // Показываем ID списка
                await DisplayAlert(
                    "✅ Успешно!",
                    $"{result.message}\n\nПользователь '{recipientUsername}' может получить список по этому ID.",
                    "OK");
            }
            else
            {
                await DisplayAlert("Ошибка", result.message, "OK");
            }
        }
        catch (Exception ex)
        {
            await Navigation.PopModalAsync();
            await DisplayAlert("Ошибка", $"Не удалось поделиться списком: {ex.Message}", "OK");
        }
    }

    private async void OnResetAllClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert(
            "Сбросить все", 
            "Вы уверены, что хотите снять все галочки?",
            "Да",
            "Отмена");

        if (confirm)
        {
            foreach (var item in items)
            {
                item.IsChecked = false;
                item.TextDecoration = TextDecorations.None;
            }
            DataService.SaveItems(listName, listId, items);
        }
    }
}