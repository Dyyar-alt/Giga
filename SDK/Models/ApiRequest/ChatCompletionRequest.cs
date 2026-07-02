using System.Text.Json.Serialization;
using GigaChatSDK.Models.Chat;
using GigaChatSDK.Models.Functions;

namespace GigaChatSDK.Models.ApiRequest
{
    /// <summary>
    /// Представляет запрос к GigaChat API для генерации ответа чата (chat completion).
    /// </summary>
    /// <remarks>
    /// Содержит сообщения диалога и параметры генерации ответа.
    /// Все параметры, кроме <see cref="Messages"/>, являются опциональными и имеют значения по умолчанию.
    /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/post-chat"/></para>
    /// </remarks>
    public class ChatCompletionRequest
    {
        /// <summary>
        /// Получает или задает название модели GigaChat для генерации ответа.
        /// </summary>
        /// <value>
        /// Название модели. По умолчанию: <c>GigaChat</c>.
        /// Другие варианты: <c>GigaChat-2</c>, <c>GigaChat-2-Pro</c>, <c>GigaChat-2-Max</c>.
        /// </value>
        [JsonPropertyName("model")]
        public string Model { get; set; }

        /// <summary>
        /// Получает или задает массив сообщений диалога.
        /// </summary>
        /// <value>Список сообщений, представляющих историю диалога.</value>
        [JsonPropertyName("messages")]
        public List<ChatMessage> Messages { get; set; }

        /// <summary>
        /// Получает или задает список функций, доступных для вызова моделью.
        /// </summary>
        /// <value>Список описаний функций или <c>null</c>, если функции не используются.</value>
        [JsonPropertyName("functions")]
        public List<FunctionDescription>? Functions { get; set; }

        /// <summary>
        /// Получает или задает режим вызова функций моделью.
        /// </summary>
        /// <value>
        /// <c>"auto"</c> — модель решает, вызывать ли функцию;
        /// <c>"none"</c> — модель не вызывает функции;
        /// объект с именем функции — принудительный вызов конкретной функции.
        /// По умолчанию: <c>"auto"</c>.
        /// </value>
        [JsonPropertyName("function_call")]
        public object? FunctionCall { get; set; }

        /// <summary>
        /// Получает или задает температуру выборки для генерации ответа.
        /// </summary>
        /// <value>
        /// Значение от 0.0 до 2.0. Чем выше значение, тем более случайным и креативным будет ответ модели.
        /// По умолчанию: <c>0.87</c>.
        /// </value>
        [JsonPropertyName("temperature")]
        public float Temperature { get; set; }

        /// <summary>
        /// Получает или задает параметр nucleus sampling (top-p) для генерации ответа.
        /// </summary>
        /// <value>
        /// Значение от 0.0 до 1.0. Задает вероятностную массу токенов, которые должна учитывать модель.
        /// Например, значение 0.1 означает, что модель будет учитывать только токены из верхних 10% по вероятности.
        /// Используется как альтернатива <see cref="Temperature"/>.
        /// По умолчанию: <c>0.47</c>.
        /// </value>
        [JsonPropertyName("top_p")]
        public float TopP { get; set; }

        /// <summary>
        /// Получает или задает количество вариантов ответов, которые нужно сгенерировать.
        /// </summary>
        /// <value>
        /// Количество вариантов ответов для каждого входного сообщения.
        /// По умолчанию: <c>1</c>.
        /// </value>
        [JsonPropertyName("n")]
        public long N { get; set; }

        /// <summary>
        /// Получает или задает, нужно ли передавать сообщения по частям в потоке (Server-Sent Events).
        /// </summary>
        /// <value>
        /// <c>true</c> — ответ передается по частям в режиме потока (SSE);
        /// <c>false</c> — ответ возвращается полностью после генерации.
        /// По умолчанию: <c>false</c>.
        /// </value>
        [JsonPropertyName("stream")]
        public bool Stream { get; set; }

        /// <summary>
        /// Получает или задает максимальное количество токенов для генерации ответа.
        /// </summary>
        /// <value>
        /// Максимальное количество токенов, которые будут использованы для создания ответа.
        /// По умолчанию: <c>512</c>.
        /// </value>
        [JsonPropertyName("max_tokens")]
        public long MaxTokens { get; set; }

        /// <summary>
        /// Получает или задает параметр повторения слов.
        /// </summary>
        /// <value>
        /// Значение 1.0 — нейтральное значение.
        /// При значении больше 1 модель будет стараться не повторять слова.
        /// Значение по умолчанию зависит от выбранной модели и может изменяться с обновлениями модели.
        /// </value>
        [JsonPropertyName("repetition_penalty")]
        public float? RepetitionPenalty { get; set; }

        /// <summary>
        /// Получает или задает минимальный интервал в секундах между отправкой токенов в потоковом режиме.
        /// </summary>
        /// <value>
        /// Параметр потокового режима (<see cref="Stream"/> = <c>true</c>).
        /// Задает минимальный интервал в секундах, который проходит между отправкой токенов.
        /// Например, если указать 1, сообщения будут приходить каждую секунду, но размер каждого из них будет больше.
        /// По умолчанию: <c>0</c>.
        /// </value>
        [JsonPropertyName("update_interval")]
        public float? UpdateInterval { get; set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ChatCompletionRequest"/> с указанными параметрами.
        /// </summary>
        /// <param name="messages">Список сообщений диалога (опционально, по умолчанию пустой список).</param>
        /// <param name="functions">Список функций, доступных для вызова моделью (опционально).</param>
        /// <param name="function_call">Режим вызова функций (опционально, по умолчанию "auto").</param>
        /// <param name="model">Название модели (опционально, по умолчанию "GigaChat").</param>
        /// <param name="temperature">Температура выборки (опционально, по умолчанию 0.87).</param>
        /// <param name="top_p">Параметр top-p (опционально, по умолчанию 0.47).</param>
        /// <param name="n">Количество вариантов ответов (опционально, по умолчанию 1).</param>
        /// <param name="stream">Режим потоковой передачи (опционально, по умолчанию false).</param>
        /// <param name="max_tokens">Максимальное количество токенов (опционально, по умолчанию 512).</param>
        /// <param name="repetition_penalty">Параметр повторения слов (опционально).</param>
        /// <param name="update_interval">Интервал обновления в потоковом режиме (опционально).</param>
        public ChatCompletionRequest(
            List<ChatMessage>? messages = null,
            List<FunctionDescription>? functions = null,
            object? function_call = null,
            string model = "GigaChat",
            float temperature = 0.87f,
            float top_p = 0.47f,
            long n = 1,
            bool stream = false,
            long max_tokens = 512,
            float? repetition_penalty = null,
            float? update_interval = null)
        {
            this.Model = model;
            // Инициализация списка сообщений: использование переданного или создание пустого
            this.Messages = messages ?? new List<ChatMessage>();
            // Инициализация списка функций: использование переданного или создание пустого
            this.Functions = functions ?? new List<FunctionDescription>();
            // Установка режима вызова функций: переданное значение или "auto" по умолчанию
            this.FunctionCall = function_call ?? "auto";
            this.Temperature = temperature;
            this.TopP = top_p;
            this.N = n;
            this.Stream = stream;
            this.MaxTokens = max_tokens;
            this.RepetitionPenalty = repetition_penalty;
            this.UpdateInterval = update_interval;
        }
    }
}
