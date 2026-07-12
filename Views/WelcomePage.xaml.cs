using Giga.ViewModels;

namespace Giga.Views;

public partial class WelcomePage : ContentPage
{
    public WelcomePage()
    {
        InitializeComponent();
    }

    private async void OnLinkTapped(object sender, EventArgs e)
    {
        try
        {
            var url = "https://developers.sber.ru/";
            await Launcher.Default.OpenAsync(url);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка открытия ссылки: {ex.Message}");
        }
    }
}