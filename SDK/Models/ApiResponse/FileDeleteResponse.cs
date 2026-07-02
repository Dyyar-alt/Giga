using System.Text.Json.Serialization;

namespace GigaChatSDK.Models.ApiResponse
{
    /// <summary>
    /// Представляет ответ API GigaChat на операцию удаления файла из хранилища.
    /// </summary>
    /// <remarks>
    /// Содержит информацию об успешности удаления и идентификатор удаленного файла.
    /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/post-file-delete"/></para>
    /// </remarks>
    public class FileDeleteResponse
    {
        /// <summary>
        /// Получает или задает идентификатор удаленного файла.
        /// </summary>
        /// <value>UUID файла в виде строки.</value>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Получает или задает флаг успешности операции удаления.
        /// </summary>
        /// <value>
        /// <c>true</c> — файл успешно удален;
        /// <c>false</c> — файл не был удален.
        /// </value>
        [JsonPropertyName("deleted")]
        public bool Deleted { get; set; }

        /// <summary>
        /// Получает или задает политику доступа удаленного файла.
        /// </summary>
        /// <value>
        /// Политика доступа файла: <c>"public"</c> или <c>"private"</c>.
        /// </value>
        [JsonPropertyName("access_policy")]
        public string AccessPolicy { get; set; }
    }
}