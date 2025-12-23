namespace MauiApp3;

public partial class RegisterPage : ContentPage
{
    private readonly AuthService _authService;

    public RegisterPage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        var username = UsernameEntry.Text?.Trim();
        var password = PasswordEntry.Text;
        var confirmPassword = ConfirmPasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(username) || 
            string.IsNullOrWhiteSpace(password) || 
            string.IsNullOrWhiteSpace(confirmPassword))
        {
            ShowMessage("Заполните все поля", false);
            return;
        }

        if (password != confirmPassword)
        {
            ShowMessage("Пароли не совпадают", false);
            return;
        }

        // Показываем загрузку
        SetLoading(true);
        HideMessage();

        var result = await _authService.RegisterAsync(username, password);

        SetLoading(false);

        if (result.success)
        {
            ShowMessage("Регистрация успешна! Переход...", true);
            
            // Небольшая задержка перед переходом
            await Task.Delay(1000);
            
            // Переход на главную страницу
            await Shell.Current.GoToAsync("//MainPage");
        }
        else
        {
            ShowMessage(result.message, false);
        }
    }

    private async void OnBackToLoginClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private void ShowMessage(string message, bool isSuccess)
    {
        MessageLabel.Text = message;
        MessageLabel.TextColor = isSuccess ? Color.FromArgb("#27AE60") : Color.FromArgb("#E74C3C");
        MessageLabel.IsVisible = true;
    }

    private void HideMessage()
    {
        MessageLabel.IsVisible = false;
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
        ConfirmPasswordEntry.Text = string.Empty;
        HideMessage();
    }
}