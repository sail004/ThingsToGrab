using System.Collections.ObjectModel;

namespace MauiApp3;

public partial class ItemsPage : ContentPage
{
    private string listName;
    private string listId;
    private ObservableCollection<ThingItem> items;

    public ItemsPage(string name, string id = null)
    {
        InitializeComponent();
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