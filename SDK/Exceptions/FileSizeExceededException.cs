using GigaChatSDK.Constants;

namespace GigaChatSDK.Exceptions
{
    /// <summary>
    /// Исключение, выбрасываемое при превышении максимально допустимого размера файла для загрузки в GigaChat API.
    /// </summary>
    public class FileSizeExceededException : GigaChatException
    {
        /// <summary>
        /// Имя файла, размер которого превышает лимит.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Категория файла (текст, изображение, аудио).
        /// </summary>
        public MimeTypeConstants.FileCategory Category { get; }

        /// <summary>
        /// Текущий размер файла в байтах.
        /// </summary>
        public long CurrentSizeBytes { get; }

        /// <summary>
        /// Максимально допустимый размер файла в байтах для данной категории.
        /// </summary>
        public long MaxSizeBytes { get; }

        /// <summary>
        /// Текущий размер файла в мегабайтах.
        /// </summary>
        public double CurrentSizeMB => CurrentSizeBytes / (1024.0 * 1024.0);

        /// <summary>
        /// Максимально допустимый размер файла в мегабайтах.
        /// </summary>
        public double MaxSizeMB => MaxSizeBytes / (1024.0 * 1024.0);

        /// <summary>
        /// Создаёт новый экземпляр исключения при превышении размера файла.
        /// </summary>
        /// <param name="fileName">Имя файла.</param>
        /// <param name="category">Категория файла.</param>
        /// <param name="currentSizeBytes">Текущий размер файла в байтах.</param>
        /// <param name="maxSizeBytes">Максимально допустимый размер в байтах.</param>
        public FileSizeExceededException(
            string fileName,
            MimeTypeConstants.FileCategory category,
            long currentSizeBytes,
            long maxSizeBytes)
            : base(BuildMessage(fileName, category, currentSizeBytes, maxSizeBytes))
        {
            FileName = fileName;
            Category = category;
            CurrentSizeBytes = currentSizeBytes;
            MaxSizeBytes = maxSizeBytes;
        }

        /// <summary>
        /// Формирует детальное сообщение об ошибке на русском языке.
        /// </summary>
        private static string BuildMessage(
            string fileName,
            MimeTypeConstants.FileCategory category,
            long currentSizeBytes,
            long maxSizeBytes)
        {
            var categoryName = category switch
            {
                MimeTypeConstants.FileCategory.Text => "текстового документа",
                MimeTypeConstants.FileCategory.Image => "изображения",
                MimeTypeConstants.FileCategory.Audio => "аудиофайла",
                _ => "файла"
            };

            var currentSizeMB = currentSizeBytes / (1024.0 * 1024.0);
            var maxSizeMB = maxSizeBytes / (1024.0 * 1024.0);

            return $"Размер {categoryName} \"{fileName}\" превышает допустимый лимит. " +
                   $"Текущий размер: {currentSizeMB:F2} МБ, максимальный: {maxSizeMB:F0} МБ.";
        }
    }
}
