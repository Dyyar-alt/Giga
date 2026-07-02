namespace GigaChatSDK.Interfaces
{
    /// <summary>
    /// Сервис для выполнения HTTP-запросов к API.
    /// </summary>
    public interface IHttpService
    {
        /// <summary>
        /// Асинхронно отправляет HTTP-запрос и возвращает содержимое ответа в виде строки.
        /// </summary>
        /// <param name="request">HTTP-запрос для отправки.</param>
        /// <returns>Содержимое ответа сервера в виде строки.</returns>
        /// <exception cref="HttpRequestException">Ошибка при отправке HTTP-запроса.</exception>
        /// <exception cref="ApplicationException">Неизвестная ошибка при выполнении запроса.</exception>
        Task<string> SendAsync(HttpRequestMessage request);

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
        HttpClientHandler CreateHttpClientHandler(bool ignoreTLS);
    }
}
