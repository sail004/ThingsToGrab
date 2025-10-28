using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MauiApp3;

public class ThingItem : INotifyPropertyChanged
{
    public string Id { get; set; }
    
    private string name;
    public string Name
    {
        get => name;
        set
        {
            name = value;
            OnPropertyChanged();
        }
    }

    private bool isChecked;
    public bool IsChecked
    {
        get => isChecked;
        set
        {
            isChecked = value;
            OnPropertyChanged();
        }
    }

    private TextDecorations textDecoration;
    public TextDecorations TextDecoration
    {
        get => textDecoration;
        set
        {
            textDecoration = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class ThingsList
{
    public string Id { get; set; }
    public string Name { get; set; }
}