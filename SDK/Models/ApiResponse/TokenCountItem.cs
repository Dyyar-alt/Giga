using System.Text.Json.Serialization;

namespace GigaChatSDK.Models.ApiResponse
{
    /// <summary>
    /// Представляет элемент подсчета токенов для одной строки текста.
    /// </summary>
    /// <remarks>
    /// Содержит количество токенов и символов для конкретной строки из запроса.
    /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/post-tokens-count"/></para>
    /// </remarks>
    public class TokenCountItem
    {
        /// <summary>
        /// Получает или задает тип объекта в ответе.
        /// </summary>
        /// <value>Обычно <c>"tokens"</c>.</value>
        [JsonPropertyName("object")]
        public string? Object { get; set; }

        /// <summary>
        /// Получает или задает количество токенов в строке.
        /// </summary>
        /// <value>Число токенов, подсчитанных моделью.</value>
        [JsonPropertyName("tokens")]
        public int Tokens { get; set; }

        /// <summary>
        /// Получает или задает количество символов в строке.
        /// </summary>
        /// <value>Количество символов в исходной строке.</value>
        [JsonPropertyName("characters")]
        public int Characters { get; set; }
    }
}
