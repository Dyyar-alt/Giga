using Giga.Services;
using Giga.ViewModels;
using Giga.Views;

#if WINDOWS
using Microsoft.UI.Xaml;
using Windows.Graphics;
#endif

namespace Giga
{
    public partial class App : Microsoft.Maui.Controls.Application  // ← ЯВНО УКАЗЫВАЕМ
    {
        public App()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("App() constructor START");

                InitializeComponent();

#if WINDOWS
                // Устанавливаем размер окна 1100x850 для Windows
                Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping("SetWindowSize", (handler, view) =>
                {
                    try
                    {
                        var nativeWindow = handler.PlatformView as Microsoft.UI.Xaml.Window;
                        if (nativeWindow != null)
                        {
                            var appWindow = nativeWindow.AppWindow;
                            if (appWindow != null)
                            {
                                appWindow.MoveAndResize(new RectInt32
                                {
                                    X = 100,
                                    Y = 100,
                                    Width = 1000,
                                    Height = 850
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка установки размера окна: {ex.Message}");
                    }
                });
#endif

                System.Diagnostics.Debug.WriteLine("InitializeComponent() OK");

                var firstLaunchService = new FirstLaunchService();
                var isFirstLaunch = firstLaunchService.IsFirstLaunch();

                System.Diagnostics.Debug.WriteLine($"Первый запуск: {isFirstLaunch}");

                if (isFirstLaunch)
                {
                    System.Diagnostics.Debug.WriteLine("Первый запуск, показываем WelcomePage...");
                    var welcomePage = new WelcomePage();
                    welcomePage.BindingContext = new WelcomeViewModel(firstLaunchService);
                    MainPage = new NavigationPage(welcomePage);
                }
                else
                {
                    string apiKey = string.Empty;
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("Создаём ApiKeyService...");
                        var keyService = new ApiKeyService();
                        System.Diagnostics.Debug.WriteLine("ApiKeyService создан");

                        System.Diagnostics.Debug.WriteLine("Получаем ключ...");
                        apiKey = keyService.GetApiKeyAsync().GetAwaiter().GetResult();
                        System.Diagnostics.Debug.WriteLine($"Ключ: {(string.IsNullOrEmpty(apiKey) ? "НЕТ" : $"ЕСТЬ ({apiKey.Length} символов)")}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка при получении ключа: {ex.Message}");
                        apiKey = string.Empty;
                    }

                    if (!string.IsNullOrEmpty(apiKey))
                    {
                        System.Diagnostics.Debug.WriteLine("Ключ есть, создаём AppShell...");
                        MainPage = new AppShell();
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Ключа нет, создаём LoginPage...");
                        var loginPage = new LoginPage();
                        loginPage.BindingContext = new LoginViewModel(new ApiKeyService());
                        MainPage = new NavigationPage(loginPage);
                    }
                }

                System.Diagnostics.Debug.WriteLine("App() constructor END");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"КРИТИЧЕСКАЯ ОШИБКА: {ex}");
                System.Diagnostics.Debug.WriteLine($"STACK: {ex.StackTrace}");

                MainPage = new ContentPage
                {
                    Content = new VerticalStackLayout
                    {
                        Padding = 20,
                        Spacing = 10,
                        Children =
                        {
                            new Label { Text = "Ошибка при запуске", FontSize = 24, TextColor = Colors.Red },
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
            System.Diagnostics.Debug.WriteLine("Приложение уходит в фон");
            base.OnSleep();
        }

        protected override void OnResume()
        {
            System.Diagnostics.Debug.WriteLine("Приложение возвращается из фона");
            base.OnResume();
        }
    }
}