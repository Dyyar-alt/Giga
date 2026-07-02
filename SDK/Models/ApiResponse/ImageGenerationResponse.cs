namespace GigaChatSDK.Models.ApiResponse
{
    /// <summary>
    /// Представляет ответ API GigaChat на запрос генерации изображения.
    /// </summary>
    /// <remarks>
    /// Содержит идентификатор сгенерированного файла и метаданные генерации.
    /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/generative-images"/></para>
    /// </remarks>
    public class ImageGenerationResponse
    {
        /// <summary>
        /// Получает или задает идентификатор сгенерированного файла изображения.
        /// </summary>
        /// <value>
        /// UUID файла в хранилище GigaChat. Используйте этот идентификатор для загрузки изображения через <c>GetImageAsByteAsync</c> или <c>DownloadFileAsync</c>.
        /// </value>
        public string FileId { get; set; }

        /// <summary>
        /// Получает или задает промпт, который был использован для генерации изображения.
        /// </summary>
        /// <value>Текстовое описание, на основе которого было сгенерировано изображение.</value>
        public string Prompt { get; set; }

        /// <summary>
        /// Получает или задает название модели, которая сгенерировала изображение.
        /// </summary>
        /// <value>Название модели (например, "GigaChat").</value>
        public string? Model { get; set; }

        /// <summary>
        /// Получает или задает время создания изображения в формате Unix timestamp (секунды).
        /// </summary>
        /// <value>Количество секунд с 1 января 1970 года (UTC).</value>
        public int Created { get; set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ImageGenerationResponse"/> (конструктор без параметров для десериализации).
        /// </summary>
        public ImageGenerationResponse()
        {
            FileId = string.Empty;
            Prompt = string.Empty;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ImageGenerationResponse"/>.
        /// </summary>
        /// <param name="fileId">Идентификатор файла изображения.</param>
        /// <param name="prompt">Промпт для генерации.</param>
        /// <param name="model">Название модели (опционально).</param>
        /// <param name="created">Время создания Unix timestamp (опционально).</param>
        public ImageGenerationResponse(string fileId, string prompt, string? model = null, int created = 0)
        {
            FileId = fileId;
            Prompt = prompt;
            Model = model;
            Created = created;
        }
    }
}
