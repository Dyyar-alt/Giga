using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace Giga;

[Activity(Theme = "@style/Maui.SplashTheme",
          MainLauncher = true,
          ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Настройка для работы с клавиатурой
        Window.SetSoftInputMode(SoftInput.AdjustResize | SoftInput.StateVisible);

        // ✅ ОТКЛЮЧАЕМ SSL НА ВСЕХ УРОВНЯХ
        try
        {
            // 1. .NET уровень
            System.Net.ServicePointManager.ServerCertificateValidationCallback =
                (sender, certificate, chain, sslPolicyErrors) => true;
            System.Net.ServicePointManager.SecurityProtocol =
                System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Tls13;
            System.Diagnostics.Debug.WriteLine("✅ .NET SSL валидация отключена");

            // 2. Android Java уровень
#if ANDROID
            Giga.Platforms.Android.SslHelper.DisableJavaSslValidation();
#endif

            // 3. Дополнительная настройка для Android
            System.Diagnostics.Debug.WriteLine("✅ Все SSL проверки отключены");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка отключения SSL: {ex.Message}");
        }
    }
}