using System.Text.Json.Serialization;

namespace GigaChatSDK.Models.ApiRequest
{
    /// <summary>
    /// Представляет запрос на подсчет количества токенов в текстовых строках.
    /// </summary>
    /// <remarks>
    /// Используется для оценки стоимости запроса перед отправкой к модели.
    /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/post-tokens-count"/></para>
    /// </remarks>
    public class TokensCountRequest
    {
        /// <summary>
        /// Получает или задает название модели, которая будет использована для подсчета токенов.
        /// </summary>
        /// <value>
        /// Идентификатор модели (например, <c>"GigaChat"</c>, <c>"GigaChat-2"</c>, <c>"GigaChat-2-Pro"</c>).
        /// </value>
        [JsonPropertyName("model")]
        public string Model { get; set; }

        /// <summary>
        /// Получает или задает массив строк, в которых необходимо подсчитать количество токенов.
        /// </summary>
        /// <value>
        /// Список текстовых строк для анализа.
        /// </value>
        [JsonPropertyName("input")]
        public List<string> Input { get; set; }

        /// <summary>
        /// Создает новый экземпляр запроса на подсчет токенов.
        /// </summary>
        public TokensCountRequest()
        {
            Model = "GigaChat";
            Input = new List<string>();
        }

        /// <summary>
        /// Создает новый экземпляр запроса на подсчет токенов с указанными параметрами.
        /// </summary>
        /// <param name="input">Массив строк для подсчета токенов.</param>
        /// <param name="model">Модель для подсчета (по умолчанию "GigaChat").</param>
        public TokensCountRequest(List<string> input, string model = "GigaChat")
        {
            Model = model;
            Input = input ?? new List<string>();
        }
    }
}
