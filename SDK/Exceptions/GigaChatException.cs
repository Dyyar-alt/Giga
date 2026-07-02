namespace GigaChatSDK.Exceptions
{
    /// <summary>
    /// Базовое исключение для всех ошибок GigaChat SDK.
    /// </summary>
    /// <remarks>
    /// Используйте этот тип для универсальной обработки всех ошибок SDK,
    /// или ловите конкретные производные исключения для специфичной обработки.
    /// </remarks>
    public class GigaChatException : Exception
    {
        /// <summary>
        /// HTTP статус-код ответа (если применимо).
        /// </summary>
        public int? StatusCode { get; }

        /// <summary>
        /// Тело ответа от сервера с деталями ошибки.
        /// </summary>
        public string? ResponseBody { get; }

        /// <summary>
        /// Создаёт новый экземпляр базового исключения GigaChat SDK.
        /// </summary>
        /// <param name="message">Сообщение об ошибке на русском языке.</param>
        public GigaChatException(string message) : base(message)
        {
        }

        /// <summary>
        /// Создаёт новый экземпляр исключения с HTTP статусом и телом ответа.
        /// </summary>
        /// <param name="message">Сообщение об ошибке на русском языке.</param>
        /// <param name="statusCode">HTTP статус-код ответа.</param>
        /// <param name="responseBody">Тело ответа от сервера.</param>
        public GigaChatException(string message, int statusCode, string? responseBody = null)
            : base(message)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }

        /// <summary>
        /// Создаёт новый экземпляр исключения с внутренним исключением.
        /// </summary>
        /// <param name="message">Сообщение об ошибке.</param>
        /// <param name="innerException">Внутреннее исключение.</param>
        public GigaChatException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
