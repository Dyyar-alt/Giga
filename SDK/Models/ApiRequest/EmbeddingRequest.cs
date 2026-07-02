using System.Text.Json.Serialization;

namespace GigaChatSDK.Models.ApiRequest
{
    /// <summary>
    /// Представляет запрос к GigaChat API для создания векторных представлений (эмбеддингов) текстовых данных.
    /// </summary>
    /// <remarks>
    /// Используется для получения эмбеддингов, которые можно использовать в задачах семантического поиска,
    /// кластеризации, классификации и других задач обработки естественного языка.
    /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/post-embeddings"/></para>
    /// </remarks>
    public class EmbeddingRequest
    {
        /// <summary>
        /// Получает или задает название модели для создания эмбеддингов.
        /// </summary>
        /// <value>
        /// Название модели. По умолчанию: <c>"Embeddings"</c>.
        /// </value>
        [JsonPropertyName("model")]
        public string Model { get; set; }

        /// <summary>
        /// Получает или задает массив текстовых строк для преобразования в векторные представления.
        /// </summary>
        /// <value>Список строк, для которых необходимо создать эмбеддинги.</value>
        [JsonPropertyName("input")]
        public List<string> Input { get; set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="EmbeddingRequest"/>.
        /// </summary>
        /// <param name="model">Название модели для создания эмбеддингов (по умолчанию "Embeddings").</param>
        /// <param name="input">Список текстовых строк для преобразования (по умолчанию пустой список).</param>
        public EmbeddingRequest(string model = "Embeddings", List<string> input = null)
        {
            this.Model = model;
            // Инициализация списка входных строк: использование переданного или создание пустого
            this.Input = input ?? new List<string>();
        }
    }
}
