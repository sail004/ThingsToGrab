using System.Collections.ObjectModel;

namespace MauiApp3;

public partial class MainPage : ContentPage
{
    private ObservableCollection<ThingsList> customLists;
    private readonly ListSharingService _sharingService;
    private readonly AuthService _authService;

    public MainPage()
    {
        InitializeComponent();
        
        // Получаем сервисы через DI
        var services = Application.Current.Handler.MauiContext.Services;
        _sharingService = services.GetService<ListSharingService>();
        _authService = services.GetService<AuthService>();
        
        LoadCustomLists();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadCustomLists();
    }

    private void LoadCustomLists()
    {
        customLists = DataService.GetCustomLists();
        CustomListsCollection.ItemsSource = customLists;
    }

    private async void OnListTapped(object sender, EventArgs e)
    {
        var frame = sender as Frame;
        var recognizer = frame?.GestureRecognizers[0] as TapGestureRecognizer;
        var listName = recognizer?.CommandParameter as string;

        if (!string.IsNullOrEmpty(listName))
        {
            await Navigation.PushAsync(new ItemsPage(listName));
        }
    }

    private async void OnCustomListTapped(object sender, EventArgs e)
    {
        var frame = sender as Frame;
        var list = frame?.BindingContext as ThingsList;

        if (list != null)
        {
            await Navigation.PushAsync(new ItemsPage(list.Name, list.Id));
        }
    }

    private async void OnCreateNewListClicked(object sender, EventArgs e)
    {
        string result = await DisplayPromptAsync(
            "Новый список", 
            "Введите название списка:",
            "Создать",
            "Отмена",
            placeholder: "Название списка");

        if (!string.IsNullOrWhiteSpace(result))
        {
            var newList = DataService.CreateCustomList(result);
            customLists.Add(newList);
            await Navigation.PushAsync(new ItemsPage(newList.Name, newList.Id));
        }
    }

    private async void OnGetSharedListClicked(object sender, EventArgs e)
    {
        if (_sharingService == null || _authService == null)
        {
            await DisplayAlert("Ошибка", "Сервисы недоступны", "OK");
            return;
        }

        // Запрашиваем ID списка
        string idString = await DisplayPromptAsync(
            "Получить список",
            "Введите ID расшаренного списка:",
            "Получить",
            "Отмена",
            placeholder: "ID",
            keyboard: Keyboard.Numeric);

        if (string.IsNullOrWhiteSpace(idString))
        {
            return;
        }

        if (!int.TryParse(idString, out int sharedListId))
        {
            await DisplayAlert("Ошибка", "ID должен быть числом", "OK");
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
            // Получаем расшаренный список
            var result = await _sharingService.GetSharedListAsync(sharedListId);

            await Navigation.PopModalAsync();

            if (result.success && result.sharedList != null)
            {
                // Спрашиваем подтверждение
                bool confirm = await DisplayAlert(
                    "Список найден!",
                    $"Название: {result.sharedList.ListName}\n" +
                    $"Владелец: {result.sharedList.Owner.Username}\n" +
                    $"Создан: {result.sharedList.CreatedAt:dd.MM.yyyy HH:mm}\n\n" +
                    $"Добавить этот список в ваши списки?",
                    "Да",
                    "Отмена");

                if (confirm)
                {
                    // Загружаем список в локальное хранилище
                    var loadResult = _sharingService.LoadSharedListToLocal(result.sharedList);

                    if (loadResult.success && loadResult.localList != null)
                    {
                        // Обновляем отображение
                        customLists.Add(loadResult.localList);

                        await DisplayAlert(
                            "✅ Успешно!",
                            "Список добавлен в ваши списки",
                            "OK");

                        // Открываем список
                        await Navigation.PushAsync(new ItemsPage(loadResult.localList.Name, loadResult.localList.Id));
                    }
                    else
                    {
                        await DisplayAlert("Ошибка", loadResult.message, "OK");
                    }
                }
            }
            else
            {
                await DisplayAlert("Ошибка", result.message, "OK");
            }
        }
        catch (Exception ex)
        {
            await Navigation.PopModalAsync();
            await DisplayAlert("Ошибка", $"Не удалось получить список: {ex.Message}", "OK");
        }
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        if (_authService == null)
        {
            return;
        }

        bool confirm = await DisplayAlert(
            "Выход",
            "Вы уверены, что хотите выйти из аккаунта?",
            "Да",
            "Отмена");

        if (confirm)
        {
            _authService.Logout();
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}