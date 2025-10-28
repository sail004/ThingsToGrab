using System.Collections.ObjectModel;

namespace MauiApp3;

public partial class MainPage : ContentPage
{
    private ObservableCollection<ThingsList> customLists;

    public MainPage()
    {
        InitializeComponent();
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
}