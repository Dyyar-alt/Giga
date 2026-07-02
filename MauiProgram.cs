using Giga.Services;
using Giga.ViewModels;
using Giga.Views;
using Microsoft.Extensions.Logging;

namespace Giga;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // ✅ Глобальное отключение SSL проверки
        System.Net.ServicePointManager.ServerCertificateValidationCallback =
            (sender, certificate, chain, sslPolicyErrors) => true;
        System.Net.ServicePointManager.SecurityProtocol =
            System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Tls13;

        // Сервисы
        builder.Services.AddSingleton<GlobalTokenHandler>();
        builder.Services.AddSingleton<IApiKeyService, ApiKeyService>();
        builder.Services.AddSingleton<ChatStorageService>();
        builder.Services.AddSingleton<IImageSaveService, ImageSaveService>();

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<ChatViewModel>();
        builder.Services.AddTransient<ImageGenerationViewModel>();

        // Страницы
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<ChatPageNew>();
        builder.Services.AddTransient<ImageGenPage>();
        builder.Services.AddTransient<AppShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}