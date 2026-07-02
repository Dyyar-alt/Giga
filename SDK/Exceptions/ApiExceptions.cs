namespace GigaChatSDK.Exceptions
{
    /// <summary>
    /// Исключение при ошибке авторизации (HTTP 401).
    /// </summary>
    /// <remarks>
    /// Выбрасывается когда токен доступа истёк (действует 30 минут) или некорректен.
    /// SDK автоматически обновляет токен при получении этой ошибки.
    /// </remarks>
    public class AuthenticationException : GigaChatException
    {
        public AuthenticationException(string message, string? responseBody = null)
            : base(message, 401, responseBody)
        {
        }
    }

    /// <summary>
    /// Исключение при недостатке токенов модели (HTTP 402).
    /// </summary>
    /// <remarks>
    /// Закончились токены модели. Необходимо пополнить баланс в личном кабинете.
    /// </remarks>
    public class InsufficientBalanceException : GigaChatException
    {
        public InsufficientBalanceException(string message, string? responseBody = null)
            : base(message, 402, responseBody)
        {
        }
    }

    /// <summary>
    /// Исключение при отсутствии доступа к ресурсу (HTTP 403).
    /// </summary>
    /// <remarks>
    /// Нет доступа к запрашиваемому ресурсу. Проверьте права доступа и тип подписки.
    /// Например, метод получения баланса доступен только при покупке пакетов токенов.
    /// </remarks>
    public class AccessForbiddenException : GigaChatException
    {
        public AccessForbiddenException(string message, string? responseBody = null)
            : base(message, 403, responseBody)
        {
        }
    }

    /// <summary>
    /// Исключение когда ресурс не найден (HTTP 404).
    /// </summary>
    /// <remarks>
    /// Запрашиваемый ресурс (файл, модель) не найден.
    /// Проверьте корректность идентификатора ресурса.
    /// </remarks>
    public class ResourceNotFoundException : GigaChatException
    {
        /// <summary>
        /// Идентификатор ресурса, который не был найден.
        /// </summary>
        public string? ResourceId { get; }

        public ResourceNotFoundException(string message, string? resourceId = null, string? responseBody = null)
            : base(message, 404, responseBody)
        {
            ResourceId = resourceId;
        }
    }

    /// <summary>
    /// Исключение при превышении максимального размера запроса (HTTP 413).
    /// </summary>
    /// <remarks>
    /// Превышен максимальный размер входных данных.
    /// Уменьшите размер промпта. Количество токенов должно быть меньше размера окна контекста модели.
    /// Используйте метод /tokens/count для оценки количества токенов.
    /// </remarks>
    public class RequestTooLargeException : GigaChatException
    {
        public RequestTooLargeException(string message, string? responseBody = null)
            : base(message, 413, responseBody)
        {
        }
    }

    /// <summary>
    /// Исключение при ошибке валидации параметров запроса (HTTP 422).
    /// </summary>
    /// <remarks>
    /// Проверьте порядок сообщений, названия полей и значения параметров.
    /// Системный промпт должен быть первым сообщением.
    /// </remarks>
    public class RequestValidationException : GigaChatException
    {
        public RequestValidationException(string message, string? responseBody = null)
            : base(message, 422, responseBody)
        {
        }
    }

    /// <summary>
    /// Исключение при превышении лимита запросов (HTTP 429).
    /// </summary>
    /// <remarks>
    /// Слишком много одновременных запросов.
    /// По умолчанию физическим лицам доступен 1 одновременный поток, ИП и юрлицам — 10.
    /// </remarks>
    public class RateLimitExceededException : GigaChatException
    {
        /// <summary>
        /// Время в секундах, через которое можно повторить запрос.
        /// </summary>
        public int? RetryAfterSeconds { get; }

        public RateLimitExceededException(string message, int? retryAfter = null, string? responseBody = null)
            : base(message, 429, responseBody)
        {
            RetryAfterSeconds = retryAfter;
        }
    }

    /// <summary>
    /// Исключение при внутренней ошибке сервиса GigaChat (HTTP 500).
    /// </summary>
    /// <remarks>
    /// Внутренняя ошибка сервиса GigaChat.
    /// Обратитесь в службу поддержки: gigachat@sberbank.ru.
    /// </remarks>
    public class ServerErrorException : GigaChatException
    {
        public ServerErrorException(string message, string? responseBody = null)
            : base(message, 500, responseBody)
        {
        }
    }
}
