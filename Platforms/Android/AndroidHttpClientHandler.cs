#if ANDROID
using Javax.Net.Ssl;
using System.Net;

namespace Giga.Platforms.Android
{
    public class AndroidHttpClientHandler : HttpClientHandler
    {
        public AndroidHttpClientHandler()
        {
            try
            {
                // Отключаем проверку сертификатов
                ServerCertificateCustomValidationCallback =
                    (sender, cert, chain, sslPolicyErrors) => true;

                // Разрешаем автоматическое перенаправление
                AllowAutoRedirect = true;

                // Используем последнюю версию TLS
                SslProtocols = System.Security.Authentication.SslProtocols.Tls12 |
                               System.Security.Authentication.SslProtocols.Tls13;

                System.Diagnostics.Debug.WriteLine("✅ AndroidHttpClientHandler создан с отключенной SSL проверкой");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка создания AndroidHttpClientHandler: {ex.Message}");
            }
        }
    }

    // Альтернативный подход через ServicePointManager
    public static class SslHelper
    {
        public static void DisableSslValidation()
        {
            try
            {
                // Глобальное отключение для всех HTTP запросов
                ServicePointManager.ServerCertificateValidationCallback =
                    (sender, certificate, chain, sslPolicyErrors) => true;

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 |
                                                       SecurityProtocolType.Tls13;

                System.Diagnostics.Debug.WriteLine("✅ SSL валидация отключена глобально");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка отключения SSL: {ex.Message}");
            }
        }

        // Для Java SSL на Android
        public static void DisableJavaSslValidation()
        {
            try
            {
                var trustManager = new TrustAllTrustManager();
                var sslContext = SSLContext.GetInstance("TLS");
                sslContext.Init(null, new ITrustManager[] { trustManager }, null);
                SSLContext.Default = sslContext;

                // Устанавливаем как основной для HttpsURLConnection
                HttpsURLConnection.DefaultSSLSocketFactory = sslContext.SocketFactory;

                System.Diagnostics.Debug.WriteLine("✅ Java SSL валидация отключена");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка отключения Java SSL: {ex.Message}");
            }
        }

        private class TrustAllTrustManager : Java.Lang.Object, ITrustManager, IX509TrustManager
        {
            public void CheckClientTrusted(Java.Security.Cert.X509Certificate[]? chain, string? authType)
            {
                // Всегда доверяем клиенту
            }

            public void CheckServerTrusted(Java.Security.Cert.X509Certificate[]? chain, string? authType)
            {
                // Всегда доверяем серверу
            }

            public Java.Security.Cert.X509Certificate[] GetAcceptedIssuers()
            {
                return new Java.Security.Cert.X509Certificate[0];
            }
        }
    }
}
#endif