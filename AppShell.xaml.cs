using Giga.Services;
using System.Windows.Input;

namespace Giga
{
    public partial class AppShell : Shell
    {
        public ICommand ClearKeyCommand { get; }
        public ICommand ExitCommand { get; }

        public AppShell()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("AppShell: Конструктор START");
                InitializeComponent();

                ClearKeyCommand = new Command(OnClearKey);
                ExitCommand = new Command(OnExit);

                BindingContext = this;

                System.Diagnostics.Debug.WriteLine("AppShell: Конструктор END");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AppShell: Ошибка в конструкторе: {ex.Message}");
                throw;
            }
        }

        private async void OnClearKey()
        {
            try
            {
                bool answer = await Application.Current!.MainPage!.DisplayAlert(
                    "Очистка ключа",
                    "Вы уверены, что хотите удалить сохраненный ключ API?\nПосле этого потребуется повторный вход.",
                    "Да, удалить",
                    "Отмена"
                );

                if (answer)
                {
                    try
                    {
                        var apiKeyService = new ApiKeyService();
                        await apiKeyService.SaveApiKeyAsync(string.Empty);

                        System.Diagnostics.Debug.WriteLine("Ключ API удален из хранилища");

                        await Application.Current!.MainPage!.DisplayAlert(
                            "Успешно",
                            "Ключ API удален. Для продолжения работы потребуется повторный вход.",
                            "ОК"
                        );

                        var loginPage = new Views.LoginPage();
                        loginPage.BindingContext = new ViewModels.LoginViewModel(new ApiKeyService());
                        Application.Current!.MainPage = new NavigationPage(loginPage);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка при очистке ключа: {ex.Message}");
                        await Application.Current!.MainPage!.DisplayAlert(
                            "Ошибка",
                            $"Не удалось удалить ключ: {ex.Message}",
                            "ОК"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в OnClearKey: {ex.Message}");
            }
        }

        private async void OnExit()
        {
            try
            {
                try
                {
                    var chatPage = this.CurrentPage as Views.ChatPageNew;
                    if (chatPage?.BindingContext is ViewModels.ChatViewModel viewModel)
                    {
                        viewModel.SaveSessionCommand?.Execute(null);
                        viewModel.Cleanup();
                        System.Diagnostics.Debug.WriteLine("Сессии сохранены перед выходом");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка сохранения сессий: {ex.Message}");
                }

                bool answer = await Application.Current!.MainPage!.DisplayAlert(
                    "Выход из приложения",
                    "Вы уверены, что хотите выйти?",
                    "Да",
                    "Нет"
                );

                if (answer)
                {
                    System.Diagnostics.Debug.WriteLine("Выход из приложения");
                    Application.Current.Quit();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в OnExit: {ex.Message}");
            }
        }
    }
}