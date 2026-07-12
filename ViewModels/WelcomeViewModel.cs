using CommunityToolkit.Mvvm.Input;
using Giga.Services;

namespace Giga.ViewModels
{
    public class WelcomeViewModel
    {
        private readonly FirstLaunchService _firstLaunchService;

        public IRelayCommand ContinueCommand { get; }

        public WelcomeViewModel(FirstLaunchService firstLaunchService)
        {
            _firstLaunchService = firstLaunchService;
            ContinueCommand = new RelayCommand(OnContinue);
        }

        private void OnContinue()
        {
            try
            {
                // Отмечаем, что первый запуск завершён
                _firstLaunchService.MarkLaunchCompleted();

                // Переходим на страницу входа
                var loginPage = new Views.LoginPage();
                loginPage.BindingContext = new LoginViewModel(new ApiKeyService());
                Application.Current!.MainPage = new NavigationPage(loginPage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка перехода: {ex.Message}");
            }
        }
    }
}