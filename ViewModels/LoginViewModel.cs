using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Giga.Services;

namespace Giga.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IApiKeyService _apiKeyService;

        [ObservableProperty]
        private string apiKey = string.Empty;

        [ObservableProperty]
        private bool isLoggingIn;

        public bool IsNotLoggingIn => !IsLoggingIn;

        public IRelayCommand LoginCommand { get; }

        public LoginViewModel(IApiKeyService apiKeyService)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("✅ LoginViewModel constructor START");
                _apiKeyService = apiKeyService;
                LoginCommand = new AsyncRelayCommand(PerformLoginAsync);
                System.Diagnostics.Debug.WriteLine("✅ LoginViewModel constructor END");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ LoginViewModel constructor EXCEPTION: {ex}");
                throw;
            }
        }
        

        partial void OnIsLoggingInChanged(bool value)
        {
            OnPropertyChanged(nameof(IsNotLoggingIn));
        }

        private async Task PerformLoginAsync()
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                await Application.Current!.MainPage!.DisplayAlert("Ошибка", "Пожалуйста, введите ключ API", "ОК");
                return;
            }

            IsLoggingIn = true;

            try
            {
                await _apiKeyService.SaveApiKeyAsync(ApiKey.Trim());
                Application.Current!.MainPage = new AppShell();
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Ошибка", $"Не удалось сохранить ключ: {ex.Message}", "ОК");
            }
            finally
            {
                IsLoggingIn = false;
            }
        }
    }
}