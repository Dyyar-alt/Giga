using GigaChatSDK.Interfaces;
using GigaChatSDK.Models.ApiRequest;
using GigaChatSDK.Models.ApiResponse;
using GigaChatSDK.Models.Chat;

namespace GigaChatSDK
{
    /// <summary>
    /// Предоставляет extension методы для упрощенного использования GigaChat API.
    /// </summary>
    public static class GigaChatExtensions
    {
        /// <summary>
        /// Отправляет простое текстовое сообщение к модели GigaChat с ролью "user".
        /// </summary>
        /// <param name="client">Экземпляр клиента GigaChat.</param>
        /// <param name="message">Текст сообщения-запроса к модели.</param>
        /// <param name="model">
        /// Название модели для генерации (по умолчанию "GigaChat").
        /// Доступные модели: GigaChat, GigaChat-2, GigaChat-2-Pro, GigaChat-2-Max.
        /// </param>
        /// <param name="ct">Токен отмены операции.</param>
        /// <returns>Ответ модели с сгенерированным текстом или <c>null</c> при ошибке.</returns>
        /// <exception cref="ArgumentNullException">Параметр <paramref name="client"/> или <paramref name="message"/> равен <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Параметр <paramref name="message"/> пустой или содержит только пробелы.</exception>
        /// <exception cref="HttpRequestException">Ошибка при выполнении HTTP-запроса к API.</exception>
        /// <remarks>
        /// Удобный метод для быстрой отправки одного сообщения без необходимости создавать <see cref="ChatCompletionRequest"/> вручную.
        /// Эквивалентен вызову <c>CreateChatCompletionAsync</c> с одним сообщением от пользователя.
        /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/post-chat"/></para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var response = await client.ChatAsync("Расскажи о себе");
        /// Console.WriteLine(response?.Choices.FirstOrDefault()?.Message.Content);
        /// </code>
        /// </example>
        public static async Task<ChatCompletionResponse?> ChatAsync(
            this IGigaChat client,
            string message,
            string model = "GigaChat",
            CancellationToken ct = default)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Сообщение не может быть пустым.", nameof(message));

            var request = new ChatCompletionRequest
            {
                Model = model
            };
            request.Messages.Add(new ChatMessage(MessageRole.User, message));

            return await client.CreateChatCompletionAsync(request, ct);
        }

        /// <summary>
        /// Отправляет запрос к модели GigaChat с историей сообщений.
        /// </summary>
        /// <param name="client">Экземпляр клиента GigaChat.</param>
        /// <param name="messages">
        /// Коллекция сообщений, представляющих историю диалога.
        /// Должна содержать минимум одно сообщение.
        /// </param>
        /// <param name="model">
        /// Название модели для генерации (по умолчанию "GigaChat").
        /// Доступные модели: GigaChat, GigaChat-2, GigaChat-2-Pro, GigaChat-2-Max.
        /// </param>
        /// <param name="temperature">
        /// Температура выборки (0.0 - 2.0). Чем выше значение, тем более креативные ответы.
        /// <c>null</c> использует значение по умолчанию модели (обычно 1.0).
        /// </param>
        /// <param name="maxTokens">
        /// Максимальное количество токенов в ответе.
        /// <c>null</c> использует значение по умолчанию модели.
        /// </param>
        /// <param name="ct">Токен отмены операции.</param>
        /// <returns>Ответ модели с сгенерированным текстом или <c>null</c> при ошибке.</returns>
        /// <exception cref="ArgumentNullException">Параметр <paramref name="client"/> или <paramref name="messages"/> равен <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Коллекция <paramref name="messages"/> пустая.</exception>
        /// <exception cref="HttpRequestException">Ошибка при выполнении HTTP-запроса к API.</exception>
        /// <remarks>
        /// Метод автоматически создает объект <see cref="ChatCompletionRequest"/> с переданными сообщениями и параметрами.
        /// Полезен для реализации многоходовых диалогов с сохранением контекста.
        /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/post-chat"/></para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var history = new List&lt;ChatMessage&gt;
        /// {
        ///     new ChatMessage(MessageRole.System, "Ты — полезный ассистент"),
        ///     new ChatMessage(MessageRole.User, "Привет!"),
        ///     new ChatMessage(MessageRole.Assistant, "Здравствуйте! Чем могу помочь?"),
        ///     new ChatMessage(MessageRole.User, "Расскажи о погоде")
        /// };
        ///
        /// var response = await client.ChatWithHistoryAsync(
        ///     messages: history,
        ///     temperature: 0.7,
        ///     maxTokens: 512
        /// );
        /// </code>
        /// </example>
        public static async Task<ChatCompletionResponse?> ChatWithHistoryAsync(
            this IGigaChat client,
            IEnumerable<ChatMessage> messages,
            string model = "GigaChat",
            float? temperature = null,
            long? maxTokens = null,
            CancellationToken ct = default)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            if (messages == null)
                throw new ArgumentNullException(nameof(messages));

            var messageList = messages.ToList();
            if (messageList.Count == 0)
                throw new ArgumentException("Коллекция сообщений не может быть пустой.", nameof(messages));

            var request = new ChatCompletionRequest
            {
                Model = model
            };

            if (temperature.HasValue)
                request.Temperature = temperature.Value;

            if (maxTokens.HasValue)
                request.MaxTokens = maxTokens.Value;

            foreach (var message in messageList)
            {
                request.Messages.Add(message);
            }

            return await client.CreateChatCompletionAsync(request, ct);
        }

        /// <summary>
        /// Извлекает текстовое содержимое первого ответа модели из <see cref="ChatCompletionResponse"/>.
        /// </summary>
        /// <param name="response">Ответ модели GigaChat.</param>
        /// <returns>
        /// Текст первого сгенерированного сообщения или <c>null</c>, если ответ пустой или не содержит сообщений.
        /// </returns>
        /// <remarks>
        /// Удобный extension метод для быстрого извлечения текста из ответа без необходимости обращаться к вложенным свойствам.
        /// Эквивалентен вызову <c>response?.Choices.FirstOrDefault()?.Message.Content</c>.
        /// </remarks>
        /// <example>
        /// <code>
        /// var response = await client.ChatAsync("Привет!");
        /// var text = response.GetContent();  // Быстрое извлечение текста
        /// Console.WriteLine(text ?? "Ответ не получен");
        /// </code>
        /// </example>
        public static string? GetContent(this ChatCompletionResponse? response)
        {
            return response?.Choices.FirstOrDefault()?.Message.Content;
        }

        /// <summary>
        /// Извлекает все текстовые ответы модели из <see cref="ChatCompletionResponse"/>.
        /// </summary>
        /// <param name="response">Ответ модели GigaChat.</param>
        /// <returns>
        /// Коллекция текстов всех сгенерированных сообщений.
        /// Пустая коллекция, если ответ пустой или не содержит сообщений.
        /// </returns>
        /// <remarks>
        /// Полезен при запросе нескольких вариантов ответа (параметр <c>N</c> в запросе).
        /// </remarks>
        /// <example>
        /// <code>
        /// var response = await client.CreateChatCompletionAsync(new ChatCompletionRequest
        /// {
        ///     Model = "GigaChat",
        ///     N = 3,  // Запросить 3 варианта ответа
        ///     Messages = { new ChatMessage(MessageRole.User, "Придумай слоган") }
        /// });
        ///
        /// var allVariants = response.GetAllContent();
        /// foreach (var variant in allVariants)
        /// {
        ///     Console.WriteLine($"- {variant}");
        /// }
        /// </code>
        /// </example>
        public static IEnumerable<string> GetAllContent(this ChatCompletionResponse? response)
        {
            if (response?.Choices == null)
                return Enumerable.Empty<string>();

            return response.Choices
                .Where(c => c?.Message?.Content != null)
                .Select(c => c.Message.Content!);
        }
    }
}
