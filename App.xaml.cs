using Giga.Services;
using Giga.ViewModels;
using Giga.Views;

namespace Giga
{
    public partial class App : Application
    {
        public App()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("✅ App() constructor START");

                InitializeComponent();
                System.Diagnostics.Debug.WriteLine("✅ InitializeComponent() OK");

                // Получаем ключ и создаем страницы
                string apiKey = string.Empty;
                try
                {
                    System.Diagnostics.Debug.WriteLine("🔧 Создаём ApiKeyService...");
                    var keyService = new ApiKeyService();
                    System.Diagnostics.Debug.WriteLine("✅ ApiKeyService создан");

                    System.Diagnostics.Debug.WriteLine("🔑 Получаем ключ...");
                    apiKey = keyService.GetApiKeyAsync().GetAwaiter().GetResult();
                    System.Diagnostics.Debug.WriteLine($"🔑 Ключ: {(string.IsNullOrEmpty(apiKey) ? "НЕТ" : $"ЕСТЬ ({apiKey.Length} символов)")}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Ошибка при получении ключа: {ex.Message}");
                    apiKey = string.Empty;
                }

                // Создаём страницу
                if (!string.IsNullOrEmpty(apiKey))
                {
                    System.Diagnostics.Debug.WriteLine("✅ Ключ есть, создаём AppShell...");
                    try
                    {
                        MainPage = new AppShell();
                        System.Diagnostics.Debug.WriteLine("✅ AppShell создан");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Ошибка создания AppShell: {ex.Message}");
                        var loginPage = new LoginPage();
                        loginPage.BindingContext = new LoginViewModel(new ApiKeyService());
                        MainPage = new NavigationPage(loginPage);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("✅ Ключа нет, создаём LoginPage...");
                    try
                    {
                        var loginPage = new LoginPage();
                        loginPage.BindingContext = new LoginViewModel(new ApiKeyService());
                        MainPage = new NavigationPage(loginPage);
                        System.Diagnostics.Debug.WriteLine("✅ LoginPage создан");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Ошибка создания LoginPage: {ex.Message}");
                        MainPage = new ContentPage
                        {
                            Content = new Label
                            {
                                Text = $"Ошибка: {ex.Message}",
                                FontSize = 24,
                                HorizontalOptions = LayoutOptions.Center,
                                VerticalOptions = LayoutOptions.Center
                            }
                        };
                    }
                }

                System.Diagnostics.Debug.WriteLine("✅ App() constructor END");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ КРИТИЧЕСКАЯ ОШИБКА: {ex}");
                System.Diagnostics.Debug.WriteLine($"❌ STACK: {ex.StackTrace}");

                MainPage = new ContentPage
                {
                    Content = new VerticalStackLayout
                    {
                        Padding = 20,
                        Spacing = 10,
                        Children =
                        {
                            new Label { Text = "❌ Ошибка при запуске", FontSize = 24, TextColor = Colors.Red },
                            new Label { Text = ex.Message, FontSize = 14 },
                            new ScrollView
                            {
                                Content = new Label
                                {
                                    Text = ex.StackTrace ?? "Нет стека",
                                    FontSize = 12,
                                    TextColor = Colors.Gray
                                },
                                HeightRequest = 300
                            }
                        }
                    }
                };
            }
        }

        protected override void OnSleep()
        {
            System.Diagnostics.Debug.WriteLine("⚠️ Приложение уходит в фон");
            base.OnSleep();
        }

        protected override void OnResume()
        {
            System.Diagnostics.Debug.WriteLine("✅ Приложение возвращается из фона");
            base.OnResume();
        }
    }
}