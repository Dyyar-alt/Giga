using System.Text.Json.Serialization;
using GigaChatSDK.Models.Chat;
using GigaChatSDK.Models.Common;

namespace GigaChatSDK.Models.ApiResponse
{
    /// <summary>
    /// Представляет ответ GigaChat API на запрос генерации текста (chat completion).
    /// </summary>
    /// <remarks>
    /// Содержит сгенерированные варианты ответов, информацию о модели и статистику использования токенов.
    /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/post-chat"/></para>
    /// </remarks>
    public class ChatCompletionResponse
    {
        /// <summary>
        /// Получает или задает список сгенерированных вариантов ответов.
        /// </summary>
        /// <value>
        /// Массив объектов <see cref="ChatChoice"/>, содержащих варианты ответов модели.
        /// Количество элементов соответствует параметру <c>n</c> в запросе.
        /// </value>
        [JsonPropertyName("choices")]
        public List<ChatChoice>? Choices { get; set; }

        /// <summary>
        /// Получает или задает время создания ответа в формате Unix timestamp (секунды).
        /// </summary>
        /// <value>Количество секунд с 1 января 1970 года (UTC).</value>
        [JsonPropertyName("created")]
        public int Created { get; set; }

        /// <summary>
        /// Получает или задает название модели, которая сгенерировала ответ.
        /// </summary>
        /// <value>Название модели (например, "GigaChat", "GigaChat-2-Pro").</value>
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        /// <summary>
        /// Получает или задает статистику использования токенов для данного запроса.
        /// </summary>
        /// <value>Объект <see cref="Usage"/> с информацией о количестве использованных токенов.</value>
        [JsonPropertyName("usage")]
        public Usage? Usage { get; set; }

        /// <summary>
        /// Получает или задает тип объекта ответа.
        /// </summary>
        /// <value>Тип объекта (обычно "chat.completion").</value>
        [JsonPropertyName("object")]
        public string? @object { get; set; }

        /// <summary>
        /// Получает или задает служебные заголовки ответа от API.
        /// </summary>
        /// <value>
        /// Объект <see cref="XRequestHeaders"/> с информацией о заголовках запроса/ответа,
        /// которые возвращаются сервером GigaChat API для логирования и трассировки.
        /// </value>
        /// <remarks>
        /// Содержит X-Request-ID, X-Session-ID и X-Client-ID из ответа сервера.
        /// Эти заголовки полезны для отладки и мониторинга взаимодействия с API.
        /// </remarks>
        [JsonIgnore]
        public XRequestHeaders? XHeaders { get; set; }
    }
}
