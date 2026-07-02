namespace GigaChatSDK.Interfaces
{
    /// <summary>
    /// Сервис управления OAuth 2.0 токенами доступа для авторизации запросов к GigaChat API.
    /// </summary>
    /// <remarks>
    /// Токен доступа действителен в течение 30 минут.
    /// Сервис автоматически обновляет токен при приближении срока истечения.
    /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/post-token"/></para>
    /// </remarks>
    public interface ITokenService
    {
        /// <summary>
        /// Получает текущий действующий токен доступа.
        /// </summary>
        /// <value>
        /// Строка с токеном доступа или <c>null</c>, если токен еще не получен.
        /// </value>
        string? CurrentToken { get; }

        /// <summary>
        /// Получает валидный токен доступа. Автоматически обновляет токен, если он истек или истекает в течение 30 секунд.
        /// </summary>
        /// <param name="ct">Токен отмены операции.</param>
        /// <returns>Действительный токен доступа для авторизации запросов к GigaChat API.</returns>
        /// <exception cref="HttpRequestException">Ошибка при получении токена от сервера авторизации.</exception>
        /// <exception cref="InvalidDataException">Получен некорректный ответ от сервера авторизации.</exception>
        Task<string> GetValidTokenAsync(CancellationToken ct);

        /// <summary>
        /// Принудительно обновляет токен доступа, запрашивая новый у сервера авторизации.
        /// </summary>
        /// <param name="ct">Токен отмены операции.</param>
        /// <returns>Новый токен доступа.</returns>
        /// <exception cref="HttpRequestException">Ошибка при получении токена от сервера авторизации.</exception>
        /// <exception cref="InvalidDataException">Получен некорректный ответ от сервера авторизации.</exception>
        /// <remarks>
        /// Используется после получения HTTP 401 Unauthorized для повторной авторизации.
        /// </remarks>
        Task<string> RefreshAsync(CancellationToken ct);
    }
}
