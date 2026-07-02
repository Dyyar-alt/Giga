using System.Text.Json.Serialization;
using GigaChatSDK.Models.Files;

namespace GigaChatSDK.Models.ApiResponse
{
    /// <summary>
    /// Представляет ответ GigaChat API на запрос списка всех файлов в хранилище.
    /// </summary>
    /// <remarks>
    /// Содержит коллекцию метаданных всех файлов, загруженных пользователем.
    /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/get-files"/></para>
    /// </remarks>
    public class FileListResponse
    {
        /// <summary>
        /// Получает или задает коллекцию метаданных файлов.
        /// </summary>
        /// <value>Коллекция объектов <see cref="GigaChatFile"/>, доступная только для чтения, содержащая информацию о файлах в хранилище.</value>
        [JsonPropertyName("data")]
        public IReadOnlyCollection<GigaChatFile> Files { get; set; }
    }
}