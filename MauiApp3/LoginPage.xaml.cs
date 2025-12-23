namespace MauiApp3;

public partial class LoginPage : ContentPage
{
    private readonly AuthService _authService;

    public LoginPage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var username = UsernameEntry.Text?.Trim();
        var password = PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ShowError("Заполните все поля");
            return;
        }

        // Показываем загрузку
        SetLoading(true);
        HideError();

        var result = await _authService.LoginAsync(username, password);

        SetLoading(false);

        if (result.success)
        {
            // Переход на главную страницу
            await Shell.Current.GoToAsync("//MainPage");
        }
        else
        {
            ShowError(result.message);
        }
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("RegisterPage");
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

    private void HideError()
    {
        ErrorLabel.IsVisible = false;
    }

    private void SetLoading(bool isLoading)
    {
        LoadingIndicator.IsRunning = isLoading;
        LoadingIndicator.IsVisible = isLoading;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Очищаем поля при появлении страницы
        UsernameEntry.Text = string.Empty;
        PasswordEntry.Text = string.Empty;
        HideError();
    }
}