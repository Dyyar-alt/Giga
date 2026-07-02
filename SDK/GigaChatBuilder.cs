using Microsoft.Extensions.Logging;

namespace GigaChatSDK
{
    /// <summary>
    /// Предоставляет fluent API для создания экземпляра <see cref="GigaChat"/> клиента.
    /// </summary>
    /// <remarks>
    /// Позволяет настроить клиент GigaChat с помощью цепочки методов для удобной конфигурации.
    /// Минимально требуется только ключ авторизации для создания клиента.
    /// </remarks>
    /// <example>
    /// <code>
    /// var client = new GigaChatBuilder()
    ///     .WithAuthKey(authKey)
    ///     .WithScope(GigaChatScope.GIGACHAT_API_PERS)
    ///     .WithLogger(logger)
    ///     .Build();
    /// </code>
    /// </example>
    public class GigaChatBuilder
    {
        private string? _authorizationKey;
        private GigaChatScope _scope = GigaChatScope.GIGACHAT_API_PERS;
        private bool _ignoreTLS = false;
        private ILogger<GigaChat>? _logger;

        /// <summary>
        /// Создает новый экземпляр построителя GigaChat клиента.
        /// </summary>
        public GigaChatBuilder()
        {
        }

        /// <summary>
        /// Устанавливает ключ авторизации для GigaChat API.
        /// </summary>
        /// <param name="authorizationKey">
        /// Ключ авторизации в формате Base64 (client_id:client_secret).
        /// Получается в личном кабинете Сбер.
        /// </param>
        /// <returns>Текущий экземпляр построителя для цепочки вызовов.</returns>
        /// <exception cref="ArgumentException">Ключ авторизации пустой или содержит только пробелы.</exception>
        public GigaChatBuilder WithAuthKey(string authorizationKey)
        {
            if (string.IsNullOrWhiteSpace(authorizationKey))
                throw new ArgumentException("Ключ авторизации не может быть пустым.", nameof(authorizationKey));

            _authorizationKey = authorizationKey;
            return this;
        }

        /// <summary>
        /// Устанавливает область доступа (scope) для GigaChat API.
        /// </summary>
        /// <param name="scope">
        /// Область доступа:
        /// <list type="bullet">
        /// <item><see cref="GigaChatScope.GIGACHAT_API_PERS"/> - для физических лиц (по умолчанию)</item>
        /// <item><see cref="GigaChatScope.GIGACHAT_API_B2B"/> - для ИП и юридических лиц</item>
        /// <item><see cref="GigaChatScope.GIGACHAT_API_CORP"/> - корпоративный доступ</item>
        /// </list>
        /// </param>
        /// <returns>Текущий экземпляр построителя для цепочки вызовов.</returns>
        public GigaChatBuilder WithScope(GigaChatScope scope)
        {
            _scope = scope;
            return this;
        }

        /// <summary>
        /// Настраивает клиент на игнорирование ошибок валидации SSL/TLS сертификатов.
        /// </summary>
        /// <param name="ignore">
        /// <c>true</c> для игнорирования ошибок SSL/TLS сертификатов;
        /// <c>false</c> для строгой проверки (по умолчанию).
        /// </param>
        /// <returns>Текущий экземпляр построителя для цепочки вызовов.</returns>
        /// <remarks>
        /// <para>⚠️ ВНИМАНИЕ: Использование этой опции снижает безопасность соединения!</para>
        /// <para>GigaChat использует сертификаты российского Минцифры, которые могут не распознаваться
        /// стандартным хранилищем сертификатов .NET. Эта опция может потребоваться в dev-окружении.</para>
        /// <para>В production рекомендуется добавить сертификаты Минцифры в хранилище доверенных сертификатов ОС.</para>
        /// </remarks>
        public GigaChatBuilder IgnoreTLS(bool ignore = true)
        {
            _ignoreTLS = ignore;
            return this;
        }

        /// <summary>
        /// Устанавливает логгер для записи диагностической информации.
        /// </summary>
        /// <param name="logger">
        /// Экземпляр <see cref="ILogger{GigaChat}"/> для логирования операций клиента.
        /// Если не указан, логирование будет отключено.
        /// </param>
        /// <returns>Текущий экземпляр построителя для цепочки вызовов.</returns>
        public GigaChatBuilder WithLogger(ILogger<GigaChat>? logger)
        {
            _logger = logger;
            return this;
        }

        /// <summary>
        /// Создает и возвращает настроенный экземпляр <see cref="GigaChat"/> клиента.
        /// </summary>
        /// <returns>Настроенный экземпляр GigaChat клиента.</returns>
        /// <exception cref="InvalidOperationException">
        /// Ключ авторизации не установлен. Используйте <see cref="WithAuthKey"/> перед вызовом <see cref="Build"/>.
        /// </exception>
        /// <remarks>
        /// После создания клиента рекомендуется вызвать <see cref="GigaChat.InitializeAsync"/> для получения токена доступа.
        /// </remarks>
        public GigaChat Build()
        {
            if (string.IsNullOrWhiteSpace(_authorizationKey))
                throw new InvalidOperationException(
                    "Ключ авторизации не установлен. Используйте метод WithAuthKey() перед вызовом Build().");

            return new GigaChat(
                authorizationKey: _authorizationKey,
                scope: _scope,
                ignoreTLS: _ignoreTLS,
                logger: _logger
            );
        }

        /// <summary>
        /// Создает новый экземпляр построителя GigaChat клиента (статический метод-фабрика).
        /// </summary>
        /// <returns>Новый экземпляр <see cref="GigaChatBuilder"/>.</returns>
        /// <example>
        /// <code>
        /// var client = GigaChatBuilder.Create()
        ///     .WithAuthKey(authKey)
        ///     .WithScope(GigaChatScope.GIGACHAT_API_PERS)
        ///     .Build();
        /// </code>
        /// </example>
        public static GigaChatBuilder Create()
        {
            return new GigaChatBuilder();
        }
    }
}
