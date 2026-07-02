using System.Text.Json.Serialization;

namespace GigaChatSDK.Models.ApiResponse
{
    /// <summary>
    /// Представляет элемент баланса токенов для конкретной модели или услуги.
    /// </summary>
    /// <remarks>
    /// Содержит название услуги/модели и количество доступных токенов.
    /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/get-balance"/></para>
    /// </remarks>
    public class BalanceItem
    {
        /// <summary>
        /// Получает или задает название модели или услуги.
        /// </summary>
        /// <value>
        /// Название модели (например, <c>"GigaChat"</c>, <c>"Embeddings"</c>) или услуги.
        /// </value>
        [JsonPropertyName("usage")]
        public string? Usage { get; set; }

        /// <summary>
        /// Получает или задает остаток токенов.
        /// </summary>
        /// <value>
        /// Количество доступных токенов для данной модели/услуги.
        /// </value>
        [JsonPropertyName("value")]
        public int Value { get; set; }
    }
}
