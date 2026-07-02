using System.Net;

namespace GigaChatSDK
{
    /// <summary>
    /// Сервис для выполнения HTTP-запросов с настройкой обработки SSL/TLS сертификатов.
    /// </summary>
    /// <remarks>
    /// Предоставляет HTTP-клиент с возможностью игнорирования ошибок валидации сертификатов,
    /// что может быть необходимо для работы в системах с проблемами сертификатов МинЦифры РФ.
    /// </remarks>
    public class HttpService
    {
        /// <summary>
        /// Получает настроенный экземпляр HTTP-клиента для выполнения запросов.
        /// </summary>
        public HttpClient Client { get; private set; }
        /// <summary>
        /// Указывает, включено ли игнорирование ошибок валидации SSL/TLS сертификатов.
        /// </summary>
        /// <remarks>
        /// <c>true</c> — игнорировать ошибки валидации сертификатов (снижает безопасность);
        /// <c>false</c> — стандартная валидация сертификатов.
        /// </remarks>
        private readonly bool ignoreTLS;

        /// <summary>
        /// Создает новый экземпляр HTTP-сервиса с указанными параметрами безопасности.
        /// </summary>
        /// <param name="ignoreTLS">
        /// <c>true</c> для игнорирования ошибок валидации SSL/TLS сертификатов;
        /// <c>false</c> для стандартной валидации.
        /// </param>
        public HttpService(bool ignoreTLS)
        {
            this.ignoreTLS = ignoreTLS;
            Client = new HttpClient(CreateHttpClientHandler(this.ignoreTLS));
        }
        /// <summary>
        /// Создает обработчик HTTP-клиента с настройками безопасности.
        /// </summary>
        /// <param name="ignoreTLS">
        /// <c>true</c> для игнорирования ошибок валидации SSL/TLS сертификатов (необходимо для систем с проблемами сертификатов МинЦифры РФ);
        /// <c>false</c> для стандартной валидации сертификатов.
        /// </param>
        /// <returns>Настроенный экземпляр <see cref="HttpClientHandler"/>.</returns>
        /// <remarks>
        /// <b>Внимание:</b> Использование <paramref name="ignoreTLS"/> = <c>true</c> снижает безопасность соединения.
        /// Рекомендуется использовать только в trusted окружениях или для разработки/тестирования.
        /// </remarks>
        public HttpClientHandler CreateHttpClientHandler(bool ignoreTLS)
        {
            var handler = new HttpClientHandler();
            if (ignoreTLS)
            {
                // Настройка игнорирования SSL/TLS сертификатов для работы с сертификатами МинЦифры РФ
                // ВНИМАНИЕ: Снижает безопасность! Использовать только в необходимых случаях
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                // Глобальный callback для всех HTTP-запросов в приложении
                ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                // Локальный callback только для данного handler
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            }
            return handler;
        }

        /// <summary>
        /// Отправляет HTTP-запрос и возвращает содержимое ответа в виде строки.
        /// </summary>
        /// <param name="request">HTTP-запрос для отправки.</param>
        /// <returns>Строковое содержимое ответа от сервера.</returns>
        /// <exception cref="ApplicationException">Ошибка при отправке HTTP-запроса или неизвестная ошибка.</exception>
        /// <exception cref="HttpRequestException">Базовая ошибка HTTP-запроса.</exception>
        public async Task<string> SendAsync(HttpRequestMessage request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            try
            {
                var response = await Client.SendAsync(request);
                // Проверка статус-кода ответа (выбросит исключение для 4xx/5xx)
                response.EnsureSuccessStatusCode();
                // Чтение содержимого ответа как строки
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                // Обработка специфичных HTTP-ошибок (сетевые проблемы, таймауты, некорректные статус-коды)
                throw new ApplicationException($"Ошибка при отправке HTTP-запроса: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                // Обработка всех остальных неожиданных исключений
                throw new ApplicationException($"Неизвестная ошибка при выполнении HTTP-запроса: {ex.Message}", ex);
            }
        }
    }
}
