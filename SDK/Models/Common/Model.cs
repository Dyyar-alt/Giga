using System.Text.Json.Serialization;

namespace GigaChatSDK.Models.Common
{
    /// <summary>
    /// Представляет метаданные модели GigaChat, доступной для использования.
    /// </summary>
    /// <remarks>
    /// Содержит информацию об идентификаторе модели, типе объекта и владельце модели.
    /// Используется при получении списка доступных моделей через API.
    /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/get-models"/></para>
    /// </remarks>
    public class Model
    {
        /// <summary>
        /// Получает или задает идентификатор модели.
        /// </summary>
        /// <value>
        /// Уникальный идентификатор модели (например, <c>"GigaChat"</c>, <c>"GigaChat-2"</c>, <c>"GigaChat-2-Pro"</c>, <c>"GigaChat-2-Max"</c>, <c>"Embeddings"</c>).
        /// </value>
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// Получает или задает тип объекта.
        /// </summary>
        /// <value>Тип объекта (обычно <c>"model"</c>).</value>
        [JsonPropertyName("object")]
        public string? @object { get; set; }

        /// <summary>
        /// Получает или задает владельца модели.
        /// </summary>
        /// <value>Организация или компания, которая владеет моделью (обычно <c>"Sber"</c> или <c>"SberDevices"</c>).</value>
        [JsonPropertyName("owned_by")]
        public string? OwnedBy { get; set; }

        /// <summary>
        /// Получает или задает тип модели.
        /// </summary>
        /// <value>
        /// Тип модели. Возможные значения:
        /// <list type="bullet">
        /// <item><description><c>"chat"</c> — модель для генерации текста</description></item>
        /// <item><description><c>"aicheck"</c> — модель для проверки текста на наличие ИИ-контента</description></item>
        /// <item><description><c>"embedder"</c> — модель для создания эмбеддингов</description></item>
        /// </list>
        /// </value>
        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }
}
