using System.Text.Json.Serialization;
using GigaChatSDK.Models.Embeddings;

namespace GigaChatSDK.Models.ApiResponse
{
    /// <summary>
    /// Представляет ответ GigaChat API на запрос создания эмбеддингов.
    /// </summary>
    /// <remarks>
    /// Содержит векторные представления для каждой строки из входного массива запроса.
    /// Индекс элемента в массиве <see cref="Data"/> соответствует индексу строки в массиве <c>input</c> запроса.
    /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/post-embeddings"/></para>
    /// </remarks>
    public class EmbeddingResponse
    {
        /// <summary>
        /// Получает или задает тип объекта ответа.
        /// </summary>
        /// <value>Тип объекта (обычно "list").</value>
        [JsonPropertyName("object")]
        public string? @object { get; set; }

        /// <summary>
        /// Получает или задает массив векторных представлений для входных текстов.
        /// </summary>
        /// <value>
        /// Список объектов <see cref="EmbeddingData"/>, содержащих векторы эмбеддингов.
        /// Индекс элемента соответствует индексу строки в исходном запросе.
        /// </value>
        [JsonPropertyName("data")]
        public List<EmbeddingData>? Data { get; set; }

        /// <summary>
        /// Получает или задает название модели, которая создала эмбеддинги.
        /// </summary>
        /// <value>Название модели (например, "Embeddings").</value>
        [JsonPropertyName("model")]
        public string? Model { get; set; }
    }
}
