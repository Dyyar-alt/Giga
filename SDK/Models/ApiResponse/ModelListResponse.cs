using System.Text.Json.Serialization;
using GigaChatSDK.Models.Common;

namespace GigaChatSDK.Models.ApiResponse
{
    /// <summary>
    /// Представляет ответ GigaChat API на запрос списка доступных моделей.
    /// </summary>
    /// <remarks>
    /// Содержит массив объектов с данными доступных моделей.
    /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/get-models"/></para>
    /// </remarks>
    public class ModelListResponse
    {
        /// <summary>
        /// Получает или задает коллекцию доступных моделей.
        /// </summary>
        /// <value>Коллекция объектов <see cref="Model"/>, содержащая информацию о доступных моделях.</value>
        [JsonPropertyName("data")]
        public IReadOnlyCollection<Model> Data { get; set; }

        /// <summary>
        /// Получает или задает тип объекта в ответе.
        /// </summary>
        /// <value>Тип объекта (обычно <c>"list"</c>).</value>
        [JsonPropertyName("object")]
        public string? Object { get; set; }
    }
}
