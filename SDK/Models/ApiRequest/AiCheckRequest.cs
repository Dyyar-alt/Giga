using System.Text.Json.Serialization;

namespace GigaChatSDK.Models.ApiRequest
{
    /// <summary>
    /// Представляет запрос на проверку текста на наличие AI-генерированного контента.
    /// </summary>
    /// <remarks>
    /// Проверка доступна только для текстов на русском языке. Минимальная длина текста — 20 слов.
    /// Метод доступен только для юридических лиц, работающих по схеме оплаты pay-as-you-go.
    /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/post-ai-check"/></para>
    /// </remarks>
    public class AiCheckRequest
    {
        /// <summary>
        /// Получает или задает название модели для проверки.
        /// </summary>
        /// <value>
        /// Доступные модели:
        /// <list type="bullet">
        /// <item><description><c>"GigaCheckClassification"</c> — разделяет текст на два класса: написанный человеком или сгенерированный нейросетью (ai/human)</description></item>
        /// <item><description><c>"GigaCheckDetection"</c> — добавляет третий класс "mixed" для текстов, частично созданных с помощью ИИ</description></item>
        /// </list>
        /// </value>
        [JsonPropertyName("model")]
        public string Model { get; set; }

        /// <summary>
        /// Получает или задает текст, который будет проверен.
        /// </summary>
        /// <value>
        /// Текст на русском языке (минимум 20 слов).
        /// </value>
        [JsonPropertyName("input")]
        public string Input { get; set; }

        /// <summary>
        /// Создает новый экземпляр запроса на проверку AI-контента.
        /// </summary>
        public AiCheckRequest()
        {
            Model = "GigaCheckClassification";
            Input = string.Empty;
        }

        /// <summary>
        /// Создает новый экземпляр запроса на проверку AI-контента с указанными параметрами.
        /// </summary>
        /// <param name="input">Текст для проверки.</param>
        /// <param name="model">Модель для проверки (по умолчанию "GigaCheckClassification").</param>
        public AiCheckRequest(string input, string model = "GigaCheckClassification")
        {
            Model = model;
            Input = input;
        }
    }
}
