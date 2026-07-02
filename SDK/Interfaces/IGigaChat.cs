using GigaChatSDK.Models.ApiRequest;
using GigaChatSDK.Models.ApiResponse;
using GigaChatSDK.Models.Common;
using GigaChatSDK.Models.Files;

namespace GigaChatSDK.Interfaces
{
    /// <summary>
    /// Основной интерфейс для взаимодействия с GigaChat API — российской нейросетевой моделью от Сбера.
    /// </summary>
    /// <remarks>
    /// GigaChat предоставляет возможности генерации текста, создания эмбеддингов и работы с файловым хранилищем.
    /// <para>Официальная документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/overview"/></para>
    /// </remarks>
    public interface IGigaChat
    {
        /// <summary>
        /// Инициализирует клиент GigaChat, получая начальный токен доступа.
        /// </summary>
        /// <param name="ct">Токен отмены операции.</param>
        /// <returns>Задача, представляющая асинхронную операцию инициализации.</returns>
        /// <remarks>
        /// Необходимо вызвать перед первым обращением к API для получения токена авторизации.
        /// </remarks>
        Task InitializeAsync(CancellationToken ct = default);

        /// <summary>
        /// Создает завершение чата на основе переданных сообщений.
        /// </summary>
        /// <param name="query">Объект запроса, содержащий сообщения и параметры генерации.</param>
        /// <param name="ct">Токен отмены операции.</param>
        /// <returns>Ответ модели с сгенерированным текстом или <c>null</c> при ошибке.</returns>
        /// <exception cref="HttpRequestException">Ошибка при выполнении HTTP-запроса к API.</exception>
        /// <remarks>
        /// Метод автоматически обновляет токен при получении HTTP 401.
        /// Имя метода соответствует стандарту OpenAI API.
        /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/post-chat"/></para>
        /// </remarks>
        Task<ChatCompletionResponse?> CreateChatCompletionAsync(ChatCompletionRequest query, CancellationToken ct = default);

        /// <summary>
        /// Создает векторные представления (эмбеддинги) для текстовых запросов.
        /// </summary>
        /// <param name="request">Запрос с массивом текстовых строк для преобразования в эмбеддинги.</param>
        /// <param name="ct">Токен отмены операции.</param>
        /// <returns>Объект с векторными представлениями текстов или <c>null</c> при ошибке.</returns>
        /// <exception cref="HttpRequestException">Ошибка при выполнении HTTP-запроса к API.</exception>
        /// <remarks>
        /// Эмбеддинги используются для семантического поиска и анализа текстов.
        /// Индекс эмбеддинга в ответе соответствует индексу строки в запросе.
        /// Имя метода соответствует стандарту OpenAI API.
        /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/post-embeddings"/></para>
        /// </remarks>
        Task<EmbeddingResponse?> CreateEmbeddingsAsync(EmbeddingRequest request, CancellationToken ct = default);

        /// <summary>
        /// Получает содержимое файла изображения из хранилища в виде массива байт.
        /// </summary>
        /// <param name="fileId">Идентификатор файла изображения в хранилище.</param>
        /// <param name="ct">Токен отмены операции.</param>
        /// <returns>Массив байт с содержимым изображения в формате JPG или <c>null</c> при ошибке.</returns>
        /// <exception cref="ArgumentException">Идентификатор файла пустой или содержит только пробелы.</exception>
        /// <exception cref="HttpRequestException">Ошибка при выполнении HTTP-запроса к API.</exception>
        /// <remarks>
        /// Используется для получения изображений, созданных встроенной функцией text2image.
        /// </remarks>
        Task<byte[]?> DownloadImageAsync(string fileId, CancellationToken ct = default);

        /// <summary>
        /// Извлекает идентификатор файла из содержимого сообщения модели.
        /// </summary>
        /// <param name="messageContent">Содержимое сообщения, содержащее HTML-тег изображения.</param>
        /// <returns>Идентификатор файла или <c>null</c>, если идентификатор не найден.</returns>
        /// <exception cref="ArgumentException">Содержимое сообщения пустое или содержит только пробелы.</exception>
        /// <remarks>
        /// Ищет шаблон <c>&lt;img src="file_id"&gt;</c> в ответе модели при генерации изображений.
        /// </remarks>
        string? GetFileId(string messageContent);

        /// <summary>
        /// Получает список всех доступных файлов в хранилище.
        /// </summary>
        /// <param name="ct">Токен отмены операции.</param>
        /// <returns>Коллекция объектов с метаданными файлов.</returns>
        /// <exception cref="HttpRequestException">Ошибка при выполнении HTTP-запроса к API.</exception>
        /// <exception cref="InvalidDataException">Получен некорректный ответ от сервера.</exception>
        /// <remarks>
        /// Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/get-files"/>
        /// </remarks>
        Task<IReadOnlyCollection<GigaChatFile>> GetFilesAsync(CancellationToken ct = default);

        /// <summary>
        /// Получает метаданные конкретного файла из хранилища по его идентификатору.
        /// </summary>
        /// <param name="fileId">Идентификатор файла в хранилище.</param>
        /// <param name="ct">Токен отмены операции.</param>
        /// <returns>Объект с метаданными файла или <c>null</c>, если файл не найден.</returns>
        /// <exception cref="HttpRequestException">Ошибка при выполнении HTTP-запроса к API.</exception>
        /// <remarks>
        /// Возвращает <c>null</c> при HTTP 404 (файл не найден).
        /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/get-file"/></para>
        /// </remarks>
        Task<GigaChatFile?> GetFileAsync(string fileId, CancellationToken ct = default);

        /// <summary>
        /// Скачивает содержимое файла из хранилища в виде потока.
        /// </summary>
        /// <param name="fileId">Идентификатор файла в хранилище.</param>
        /// <param name="ct">Токен отмены операции.</param>
        /// <returns>Поток с содержимым файла.</returns>
        /// <exception cref="HttpRequestException">Ошибка при выполнении HTTP-запроса к API.</exception>
        /// <remarks>
        /// Используется для скачивания изображений, 3D-моделей и других файлов из хранилища.
        /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/get-file-content"/></para>
        /// </remarks>
        Task<Stream> DownloadFileAsync(string fileId, CancellationToken ct = default);

        /// <summary>
        /// Загружает файл в хранилище GigaChat.
        /// </summary>
        /// <param name="fileStream">Поток с содержимым файла.</param>
        /// <param name="fileName">Имя файла с расширением.</param>
        /// <param name="ct">Токен отмены операции.</param>
        /// <returns>Объект с метаданными загруженного файла, включая присвоенный идентификатор.</returns>
        /// <exception cref="HttpRequestException">Ошибка при выполнении HTTP-запроса к API.</exception>
        /// <exception cref="InvalidDataException">Получен некорректный ответ от сервера.</exception>
        /// <remarks>
        /// Поддерживаются текстовые документы, изображения и аудиофайлы.
        /// Максимальный размер: текст — 40 МБ, аудио — 35 МБ, изображения — 15 МБ.
        /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/post-file"/></para>
        /// </remarks>
        Task<GigaChatFile> CreateFileAsync(Stream fileStream, string fileName, CancellationToken ct = default);

        /// <summary>
        /// Удаляет файл из хранилища по его идентификатору.
        /// </summary>
        /// <param name="fileId">Идентификатор файла для удаления.</param>
        /// <param name="ct">Токен отмены операции.</param>
        /// <returns>Результат операции удаления.</returns>
        /// <exception cref="FileNotFoundException">Файл с указанным идентификатором не найден.</exception>
        /// <exception cref="HttpRequestException">Ошибка при выполнении HTTP-запроса к API.</exception>
        /// <remarks>
        /// Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/delete-file"/>
        /// </remarks>
        Task<FileDeleteResponse> DeleteFileAsync(string fileId, CancellationToken ct = default);

        /// <summary>
        /// Получает список всех доступных моделей GigaChat.
        /// </summary>
        /// <param name="ct">Токен отмены операции.</param>
        /// <returns>Коллекция объектов с метаданными доступных моделей.</returns>
        /// <exception cref="HttpRequestException">Ошибка при выполнении HTTP-запроса к API.</exception>
        /// <remarks>
        /// Возвращает информацию о всех доступных моделях, включая модели для генерации текста,
        /// создания эмбеддингов и проверки ИИ-контента.
        /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/get-models"/></para>
        /// </remarks>
        Task<IReadOnlyCollection<Model>> GetModelsAsync(CancellationToken ct = default);

        /// <summary>
        /// Получает информацию о конкретной модели GigaChat по её идентификатору.
        /// </summary>
        /// <param name="modelId">Идентификатор модели (например, "GigaChat", "GigaChat-2-Pro", "Embeddings", "EmbeddingsGigaR").</param>
        /// <param name="ct">Токен отмены операции.</param>
        /// <returns>Объект с метаданными указанной модели или <c>null</c>, если модель не найдена.</returns>
        /// <exception cref="ArgumentException">Идентификатор модели пустой или содержит только пробелы.</exception>
        /// <exception cref="HttpRequestException">Ошибка при выполнении HTTP-запроса к API.</exception>
        /// <remarks>
        /// Возвращает подробную информацию о модели, включая её тип, владельца и другие метаданные.
        /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/get-model"/></para>
        /// </remarks>
        Task<Model?> GetModelAsync(string modelId, CancellationToken ct = default);

        /// <summary>
        /// Подсчитывает количество токенов в текстовых строках.
        /// </summary>
        /// <param name="request">Запрос с массивом строк и названием модели для подсчета.</param>
        /// <param name="ct">Токен отмены операции.</param>
        /// <returns>Объект с информацией о количестве токенов для каждой строки.</returns>
        /// <exception cref="HttpRequestException">Ошибка при выполнении HTTP-запроса к API.</exception>
        /// <remarks>
        /// Позволяет заранее оценить стоимость запроса перед отправкой к модели.
        /// Индекс элемента в ответе соответствует индексу строки в запросе.
        /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/post-tokens-count"/></para>
        /// </remarks>
        Task<TokensCountResponse> CountTokensAsync(TokensCountRequest request, CancellationToken ct = default);

        /// <summary>
        /// Получает остаток токенов для каждой доступной модели.
        /// </summary>
        /// <param name="ct">Токен отмены операции.</param>
        /// <returns>Объект с информацией об остатках токенов по моделям.</returns>
        /// <exception cref="HttpRequestException">Ошибка при выполнении HTTP-запроса к API.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">HTTP 403 - метод недоступен при оплате pay-as-you-go.</exception>
        /// <remarks>
        /// Метод доступен только при покупке пакетов токенов.
        /// Если вы оплачиваете работу с API по схеме pay-as-you-go, запрос вернет ошибку 403 Permission Denied.
        /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/get-balance"/></para>
        /// </remarks>
        Task<BalanceResponse> GetBalanceAsync(CancellationToken ct = default);

        /// <summary>
        /// Проверяет текст на наличие AI-генерированного контента.
        /// </summary>
        /// <param name="request">Запрос с текстом и моделью для проверки.</param>
        /// <param name="ct">Токен отмены операции.</param>
        /// <returns>Результат проверки с категорией текста и найденными AI-фрагментами.</returns>
        /// <exception cref="HttpRequestException">Ошибка при выполнении HTTP-запроса к API.</exception>
        /// <remarks>
        /// Проверка доступна только для текстов на русском языке (минимум 20 слов).
        /// Метод доступен только для юридических лиц, работающих по схеме оплаты pay-as-you-go.
        /// <para>Доступные модели:</para>
        /// <list type="bullet">
        /// <item><c>GigaCheckClassification</c> — определяет, написан ли текст человеком или AI (ai/human)</item>
        /// <item><c>GigaCheckDetection</c> — дополнительно определяет смешанный контент (mixed)</item>
        /// </list>
        /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/post-ai-check"/></para>
        /// </remarks>
        Task<AiCheckResponse> CheckAiContentAsync(AiCheckRequest request, CancellationToken ct = default);

        /// <summary>
        /// Генерирует изображение по текстовому описанию и возвращает структурированный результат.
        /// </summary>
        /// <param name="prompt">Текстовое описание изображения, которое нужно сгенерировать.</param>
        /// <param name="model">Модель для генерации (по умолчанию "GigaChat").</param>
        /// <param name="systemPrompt">Системный промпт для стилизации изображения (например, "Ты — Василий Кандинский").</param>
        /// <param name="ct">Токен отмены операции.</param>
        /// <returns>Объект <see cref="ImageGenerationResponse"/> с идентификатором файла и метаданными или <c>null</c> при ошибке.</returns>
        /// <exception cref="ArgumentException">Промпт пустой или содержит только пробелы.</exception>
        /// <exception cref="HttpRequestException">Ошибка при выполнении HTTP-запроса к API.</exception>
        /// <exception cref="InvalidOperationException">Не удалось извлечь идентификатор файла из ответа модели.</exception>
        /// <remarks>
        /// Метод автоматически извлекает идентификатор файла из ответа модели, избавляя от необходимости парсить HTML вручную.
        /// Для загрузки сгенерированного изображения используйте методы <see cref="DownloadImageAsync"/> или <see cref="DownloadFileAsync"/> с полученным FileId.
        /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/guides/images-generation"/></para>
        /// </remarks>
        Task<ImageGenerationResponse?> CreateImageAsync(string prompt, string model = "GigaChat", string? systemPrompt = null, CancellationToken ct = default);

        /// <summary>
        /// Распознает и анализирует содержимое изображения по заданному промпту.
        /// </summary>
        /// <param name="fileId">Идентификатор файла изображения в хранилище.</param>
        /// <param name="prompt">Промпт для описания задачи распознавания (например, "Опиши, что изображено на фото").</param>
        /// <param name="model">Модель для распознавания (по умолчанию "GigaChat-2-Max").</param>
        /// <param name="ct">Токен отмены операции.</param>
        /// <returns>Текстовое описание содержимого изображения.</returns>
        /// <exception cref="ArgumentException">Идентификатор файла или промпт пустые.</exception>
        /// <exception cref="HttpRequestException">Ошибка при выполнении HTTP-запроса к API.</exception>
        /// <remarks>
        /// Требуется модель с поддержкой vision (например, GigaChat-2-Max).
        /// Для распознавания передает файл через параметр Attachments в сообщении.
        /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/post-chat"/></para>
        /// </remarks>
        Task<string> RecognizeImageAsync(string fileId, string prompt = "Опиши, что изображено на фото", string model = "GigaChat-2-Max", CancellationToken ct = default);
    }
}
