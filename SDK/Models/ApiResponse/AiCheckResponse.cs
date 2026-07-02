using System.Text.Json.Serialization;

namespace GigaChatSDK.Models.ApiResponse
{
    /// <summary>
    /// Представляет ответ API на запрос проверки текста на AI-контент.
    /// </summary>
    /// <remarks>
    /// Содержит результат классификации текста и детальную информацию о найденных AI-фрагментах.
    /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/post-ai-check"/></para>
    /// </remarks>
    public class AiCheckResponse
    {
        /// <summary>
        /// Получает или задает результат проверки текста.
        /// </summary>
        /// <value>
        /// Возможные значения:
        /// <list type="bullet">
        /// <item><description><c>"ai"</c> — текст сгенерирован с помощью нейросетевых моделей</description></item>
        /// <item><description><c>"human"</c> — текст написан человеком</description></item>
        /// <item><description><c>"mixed"</c> — текст содержит фрагменты, написанные человеком и сгенерированные моделью</description></item>
        /// </list>
        /// </value>
        [JsonPropertyName("category")]
        public string? Category { get; set; }

        /// <summary>
        /// Получает или задает количество символов в переданном тексте.
        /// </summary>
        /// <value>Общее количество символов.</value>
        [JsonPropertyName("characters")]
        public int Characters { get; set; }

        /// <summary>
        /// Получает или задает количество токенов в переданном тексте.
        /// </summary>
        /// <value>Количество токенов, подсчитанных моделью.</value>
        [JsonPropertyName("tokens")]
        public int Tokens { get; set; }

        /// <summary>
        /// Получает или задает части текста, сгенерированные моделью.
        /// </summary>
        /// <value>
        /// Массив интервалов, где каждый интервал представлен массивом из двух чисел:
        /// начальный и конечный индексы символов сгенерированного фрагмента.
        /// Содержит пустой массив, если текст полностью сгенерирован AI или полностью написан человеком.
        /// </value>
        /// <example>
        /// [[0, 100], [150, 200]] означает, что символы 0-100 и 150-200 сгенерированы AI.
        /// </example>
        [JsonPropertyName("ai_intervals")]
        public List<List<int>>? AiIntervals { get; set; }
    }
}
