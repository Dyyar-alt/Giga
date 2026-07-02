namespace GigaChatSDK.Constants
{
    /// <summary>
    /// Константы MIME-типов, используемые для определения типов файлов при загрузке в GigaChat API.
    /// </summary>
    public static class MimeTypeConstants
    {
        /// <summary>
        /// Максимальный размер текстового документа в байтах (40 МБ).
        /// </summary>
        public const long MaxTextDocumentSizeBytes = 40 * 1024 * 1024;

        /// <summary>
        /// Максимальный размер изображения в байтах (15 МБ).
        /// </summary>
        public const long MaxImageSizeBytes = 15 * 1024 * 1024;

        /// <summary>
        /// Максимальный размер аудиофайла в байтах (35 МБ).
        /// </summary>
        public const long MaxAudioSizeBytes = 35 * 1024 * 1024;

        /// <summary>
        /// Категории файлов для валидации размера.
        /// </summary>
        public enum FileCategory
        {
            /// <summary>Текстовый документ</summary>
            Text,
            /// <summary>Изображение</summary>
            Image,
            /// <summary>Аудиофайл</summary>
            Audio,
            /// <summary>Неизвестный тип</summary>
            Unknown
        }

        /// <summary>
        /// Словарь сопоставления расширений файлов с соответствующими MIME-типами для GigaChat API.
        /// </summary>
        /// <remarks>
        /// Ключи регистронезависимы. Поддерживаются текстовые документы, изображения и аудиофайлы.
        /// <para><b>Важно:</b> MIME-типы соответствуют спецификации GigaChat API и могут отличаться от стандартных IANA типов.</para>
        /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/post-file"/></para>
        /// </remarks>
        public static readonly Dictionary<string, string> GigaChatMime = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // =============== Текстовые документы (макс. 40 МБ) ===============

            [".txt"] = "text/plain",

            [".doc"] = "application/msword",

            [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",

            [".pdf"] = "application/pdf",

            // EPUB: стандарт IANA - application/epub+zip
            [".epub"] = "application/epub",

            // PowerPoint (старый): стандарт - application/vnd.ms-powerpoint
            [".ppt"] = "application/ppt",

            // PowerPoint (новый): стандарт - application/vnd.openxmlformats-officedocument.presentationml.presentation
            [".pptx"] = "application/pptx",

            // =============== Изображения (макс. 15 МБ) ===============

            [".jpeg"] = "image/jpeg",
            [".jpg"] = "image/jpeg",

            [".png"] = "image/png",

            [".tiff"] = "image/tiff",
            [".tif"] = "image/tiff",

            // BMP: также используется image/x-ms-bmp
            [".bmp"] = "image/bmp",

            // =============== Аудиофайлы (макс. 35 МБ) ===============

            // MP4 Audio: стандарт - audio/mp4, но часто используется video/mp4
            [".mp4"] = "audio/mp4",

            // MP3: стандарт IANA - audio/mpeg
            [".mp3"] = "audio/mp3",

            [".m4a"] = "audio/x-m4a",

            // WAV: стандартные варианты - audio/wav, audio/wave, audio/x-pn-wav
            [".wav"] = "audio/x-wav",

            // WebM Audio: стандарт - audio/webm
            [".weba"] = "audio/webm",

            // Ogg: стандарт IANA - audio/ogg
            [".ogg"] = "audio/x-ogg",

            [".opus"] = "audio/opus",
        };

        /// <summary>
        /// Определяет категорию файла по его расширению.
        /// </summary>
        /// <param name="fileName">Имя файла с расширением.</param>
        /// <returns>Категория файла для валидации размера.</returns>
        public static FileCategory GetFileCategory(string fileName)
        {
            var ext = System.IO.Path.GetExtension(fileName)?.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(ext))
                return FileCategory.Unknown;

            // Текстовые документы
            if (ext == ".txt" || ext == ".doc" || ext == ".docx" ||
                ext == ".pdf" || ext == ".epub" || ext == ".ppt" || ext == ".pptx")
                return FileCategory.Text;

            // Изображения
            if (ext == ".jpeg" || ext == ".jpg" || ext == ".png" ||
                ext == ".tiff" || ext == ".tif" || ext == ".bmp")
                return FileCategory.Image;

            // Аудиофайлы
            if (ext == ".mp4" || ext == ".mp3" || ext == ".m4a" ||
                ext == ".wav" || ext == ".weba" || ext == ".ogg" || ext == ".opus")
                return FileCategory.Audio;

            return FileCategory.Unknown;
        }

        /// <summary>
        /// Получает максимально допустимый размер файла в байтах для указанной категории.
        /// </summary>
        /// <param name="category">Категория файла.</param>
        /// <returns>Максимальный размер в байтах.</returns>
        public static long GetMaxSizeBytes(FileCategory category)
        {
            return category switch
            {
                FileCategory.Text => MaxTextDocumentSizeBytes,
                FileCategory.Image => MaxImageSizeBytes,
                FileCategory.Audio => MaxAudioSizeBytes,
                _ => long.MaxValue
            };
        }
    }
}
