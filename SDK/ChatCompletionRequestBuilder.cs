using GigaChatSDK.Models.ApiRequest;
using GigaChatSDK.Models.Chat;
using GigaChatSDK.Models.Functions;

namespace GigaChatSDK
{
    /// <summary>
    /// Предоставляет fluent API для создания объекта <see cref="ChatCompletionRequest"/>.
    /// </summary>
    /// <remarks>
    /// Позволяет постепенно настроить запрос к GigaChat API с помощью цепочки методов.
    /// </remarks>
    /// <example>
    /// <code>
    /// var request = ChatCompletionRequestBuilder.Create()
    ///     .WithModel("GigaChat-2-Pro")
    ///     .AddSystemMessage("Ты — полезный ассистент")
    ///     .AddUserMessage("Привет!")
    ///     .WithTemperature(0.7f)
    ///     .Build();
    /// </code>
    /// </example>
    public class ChatCompletionRequestBuilder
    {
        private readonly ChatCompletionRequest _request;

        /// <summary>
        /// Создает новый экземпляр построителя запроса чата.
        /// </summary>
        public ChatCompletionRequestBuilder()
        {
            _request = new ChatCompletionRequest();
        }

        /// <summary>
        /// Создает новый экземпляр построителя запроса чата (статический метод-фабрика).
        /// </summary>
        /// <returns>Новый экземпляр <see cref="ChatCompletionRequestBuilder"/>.</returns>
        public static ChatCompletionRequestBuilder Create()
        {
            return new ChatCompletionRequestBuilder();
        }

        /// <summary>
        /// Устанавливает модель для генерации ответа.
        /// </summary>
        /// <param name="model">
        /// Название модели (например, "GigaChat", "GigaChat-2", "GigaChat-2-Pro", "GigaChat-2-Max").
        /// </param>
        /// <returns>Текущий экземпляр построителя для цепочки вызовов.</returns>
        public ChatCompletionRequestBuilder WithModel(string model)
        {
            _request.Model = model;
            return this;
        }

        /// <summary>
        /// Добавляет сообщение в историю диалога.
        /// </summary>
        /// <param name="role">Роль отправителя (user, assistant, system).</param>
        /// <param name="content">Текст сообщения.</param>
        /// <returns>Текущий экземпляр построителя для цепочки вызовов.</returns>
        /// <exception cref="ArgumentException">Роль или содержимое пустые.</exception>
        public ChatCompletionRequestBuilder AddMessage(string role, string content)
        {
            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentException("Роль не может быть пустой.", nameof(role));
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Содержимое не может быть пустым.", nameof(content));

            _request.Messages.Add(new ChatMessage(role, content));
            return this;
        }

        /// <summary>
        /// Добавляет сообщение в историю диалога.
        /// </summary>
        /// <param name="role">Роль отправителя из enum <see cref="MessageRole"/>.</param>
        /// <param name="content">Текст сообщения.</param>
        /// <returns>Текущий экземпляр построителя для цепочки вызовов.</returns>
        /// <exception cref="ArgumentException">Содержимое пустое.</exception>
        public ChatCompletionRequestBuilder AddMessage(MessageRole role, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Содержимое не может быть пустым.", nameof(content));

            _request.Messages.Add(new ChatMessage(role, content));
            return this;
        }

        /// <summary>
        /// Добавляет системное сообщение (инструкцию для модели).
        /// </summary>
        /// <param name="content">Текст системного сообщения.</param>
        /// <returns>Текущий экземпляр построителя для цепочки вызовов.</returns>
        /// <exception cref="ArgumentException">Содержимое пустое.</exception>
        public ChatCompletionRequestBuilder AddSystemMessage(string content)
        {
            return AddMessage(MessageRole.System, content);
        }

        /// <summary>
        /// Добавляет сообщение пользователя.
        /// </summary>
        /// <param name="content">Текст сообщения пользователя.</param>
        /// <returns>Текущий экземпляр построителя для цепочки вызовов.</returns>
        /// <exception cref="ArgumentException">Содержимое пустое.</exception>
        public ChatCompletionRequestBuilder AddUserMessage(string content)
        {
            return AddMessage(MessageRole.User, content);
        }

        /// <summary>
        /// Добавляет сообщение ассистента (предыдущий ответ модели).
        /// </summary>
        /// <param name="content">Текст сообщения ассистента.</param>
        /// <returns>Текущий экземпляр построителя для цепочки вызовов.</returns>
        /// <exception cref="ArgumentException">Содержимое пустое.</exception>
        public ChatCompletionRequestBuilder AddAssistantMessage(string content)
        {
            return AddMessage(MessageRole.Assistant, content);
        }

        /// <summary>
        /// Добавляет коллекцию сообщений в историю диалога.
        /// </summary>
        /// <param name="messages">Коллекция сообщений для добавления.</param>
        /// <returns>Текущий экземпляр построителя для цепочки вызовов.</returns>
        /// <exception cref="ArgumentNullException">Коллекция сообщений null.</exception>
        public ChatCompletionRequestBuilder AddMessages(IEnumerable<ChatMessage> messages)
        {
            if (messages == null)
                throw new ArgumentNullException(nameof(messages));

            foreach (var message in messages)
            {
                _request.Messages.Add(message);
            }

            return this;
        }

        /// <summary>
        /// Устанавливает температуру выборки (0.0 - 2.0).
        /// </summary>
        /// <param name="temperature">
        /// Температура выборки. Чем выше значение, тем более креативные и разнообразные ответы.
        /// Рекомендуемые значения: 0.7-1.0.
        /// </param>
        /// <returns>Текущий экземпляр построителя для цепочки вызовов.</returns>
        public ChatCompletionRequestBuilder WithTemperature(float temperature)
        {
            _request.Temperature = temperature;
            return this;
        }

        /// <summary>
        /// Устанавливает максимальное количество токенов в ответе.
        /// </summary>
        /// <param name="maxTokens">Максимальное количество токенов.</param>
        /// <returns>Текущий экземпляр построителя для цепочки вызовов.</returns>
        public ChatCompletionRequestBuilder WithMaxTokens(long maxTokens)
        {
            _request.MaxTokens = maxTokens;
            return this;
        }

        /// <summary>
        /// Устанавливает параметр Top-p для nucleus sampling.
        /// </summary>
        /// <param name="topP">
        /// Вероятностный порог для nucleus sampling (0.0 - 1.0).
        /// Используется как альтернатива температуре.
        /// </param>
        /// <returns>Текущий экземпляр построителя для цепочки вызовов.</returns>
        public ChatCompletionRequestBuilder WithTopP(float topP)
        {
            _request.TopP = topP;
            return this;
        }

        /// <summary>
        /// Устанавливает параметр повторения слов (Repetition Penalty).
        /// </summary>
        /// <param name="repetitionPenalty">
        /// Штраф за повторение слов. Значение 1.0 — нейтральное.
        /// При значении больше 1 модель будет стараться не повторять слова.
        /// </param>
        /// <returns>Текущий экземпляр построителя для цепочки вызовов.</returns>
        public ChatCompletionRequestBuilder WithRepetitionPenalty(float repetitionPenalty)
        {
            _request.RepetitionPenalty = repetitionPenalty;
            return this;
        }

        /// <summary>
        /// Устанавливает количество вариантов ответа для генерации.
        /// </summary>
        /// <param name="n">Количество альтернативных ответов (обычно 1-5).</param>
        /// <returns>Текущий экземпляр построителя для цепочки вызовов.</returns>
        public ChatCompletionRequestBuilder WithN(int n)
        {
            _request.N = n;
            return this;
        }

        /// <summary>
        /// Добавляет описание функции, которую модель может вызвать.
        /// </summary>
        /// <param name="function">Описание функции.</param>
        /// <returns>Текущий экземпляр построителя для цепочки вызовов.</returns>
        /// <exception cref="ArgumentNullException">Функция null.</exception>
        public ChatCompletionRequestBuilder AddFunction(FunctionDescription function)
        {
            if (function == null)
                throw new ArgumentNullException(nameof(function));

            _request.Functions.Add(function);
            return this;
        }

        /// <summary>
        /// Устанавливает режим вызова функций.
        /// </summary>
        /// <param name="functionCall">
        /// Режим вызова функций:
        /// <list type="bullet">
        /// <item>"auto" - модель сама решает, вызывать ли функцию (по умолчанию)</item>
        /// <item>"none" - модель не будет вызывать функции</item>
        /// </list>
        /// </param>
        /// <returns>Текущий экземпляр построителя для цепочки вызовов.</returns>
        public ChatCompletionRequestBuilder WithFunctionCall(string functionCall)
        {
            _request.FunctionCall = functionCall;
            return this;
        }

        /// <summary>
        /// Создает и возвращает настроенный объект <see cref="ChatCompletionRequest"/>.
        /// </summary>
        /// <returns>Настроенный объект запроса чата.</returns>
        /// <exception cref="InvalidOperationException">
        /// Запрос не содержит ни одного сообщения.
        /// </exception>
        public ChatCompletionRequest Build()
        {
            if (_request.Messages.Count == 0)
                throw new InvalidOperationException(
                    "Запрос должен содержать хотя бы одно сообщение. Используйте методы AddMessage, AddUserMessage или AddSystemMessage.");

            return _request;
        }
    }
}
