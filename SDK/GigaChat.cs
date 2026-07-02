using System.Text.RegularExpressions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GigaChatSDK.Models.ApiRequest;
using GigaChatSDK.Models.ApiResponse;
using GigaChatSDK.Models.Chat;
using GigaChatSDK.Models.Common;
using GigaChatSDK.Models.Files;
using GigaChatSDK.Interfaces;
using System.Text;
using GigaChatSDK.Constants;
using Microsoft.Extensions.Logging;
using System.Reflection;
using GigaChatSDK.Exceptions;

namespace GigaChatSDK
{
    /// <summary>
    /// Основная реализация клиента для взаимодействия с GigaChat API — российской нейросетевой моделью от Сбера.
    /// </summary>
    /// <remarks>
    /// Класс предоставляет методы для генерации текста, создания эмбеддингов и работы с файловым хранилищем.
    /// Автоматически управляет токенами доступа и обновляет их при получении HTTP 401.
    /// <para>Официальная документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/overview"/></para>
    /// </remarks>
    public class GigaChat : IGigaChat
    {
        private const string baseUrl = "https://gigachat.devices.sberbank.ru/api/v1";
        private readonly HttpClient _httpClient;
        private readonly ITokenService _tokenService;
        private readonly ILogger<GigaChat> _logger;
        private static string? _cachedClientId;

        /// <summary>
        /// Дополнительные заголовки для запросов к API (X-Session-ID, X-Request-ID, X-Client-ID).
        /// </summary>
        public XRequestHeaders? RequestHeaders { get; set; }

        /// <summary>
        /// Создает новый экземпляр клиента GigaChat.
        /// </summary>
        /// <param name="authorizationKey">Ключ авторизации в формате Base64 для заголовка Authorization (Basic).</param>
        /// <param name="scope">
        /// Область доступа (scope) для OAuth токена:
        /// <c>GIGACHAT_API_PERS</c> - персональный доступ (физические лица);
        /// <c>GIGACHAT_API_B2B</c> - B2B доступ (ИП и юр. лица, предоплатные пакеты);
        /// <c>GIGACHAT_API_CORP</c> - корпоративный доступ (постоплата).
        /// </param>
        /// <param name="ignoreTLS">
        /// <c>true</c> для игнорирования ошибок валидации SSL/TLS сертификатов;
        /// <c>false</c> для стандартной валидации.
        /// </param>
        /// <param name="logger">Логгер для записи событий и ошибок.</param>
        public GigaChat(string authorizationKey, GigaChatScope scope, bool ignoreTLS, ILogger<GigaChat> logger)
        {
            if (string.IsNullOrWhiteSpace(authorizationKey))
                throw new ArgumentException("Ключ авторизации не может быть пустым.", nameof(authorizationKey));

            _logger = logger;
            var handler = new HttpClientHandler();
            if (ignoreTLS)
            {
                // Настройка игнорирования SSL/TLS сертификатов для работы с сертификатами МинЦифры РФ
                // ВНИМАНИЕ: Снижает безопасность! Использовать только в необходимых случаях
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                // Глобальный callback для всех HTTP-запросов
                ServicePointManager.ServerCertificateValidationCallback =
                    (sender, cert, chain, sslPolicyErrors) => true;
                // Локальный callback только для данного handler
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            }

            // Инициализация HTTP-клиента для работы с GigaChat API
            _httpClient = new HttpClient(handler);
            // Создание сервиса управления токенами с теми же настройками безопасности
            _tokenService = new TokenService(authorizationKey, scope, ignoreTLS, logger);

            // Автоматическая установка X-Client-ID по умолчанию
            RequestHeaders = new XRequestHeaders
            {
                XClientId = GetDefaultClientId()
            };
        }

        /// <summary>
        /// Получает строку X-Client-ID для SDK в формате "GigaChatSDK/версия".
        /// </summary>
        /// <returns>Строка X-Client-ID.</returns>
        /// <remarks>
        /// X-Client-ID автоматически включается в запросы для идентификации клиентской библиотеки.
        /// Формат: "GigaChatSDK/major.minor.patch"
        /// </remarks>
        private static string GetDefaultClientId()
        {
            if (_cachedClientId != null)
            {
                return _cachedClientId;
            }

            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version;

                // Формируем версию в формате major.minor.patch
                var versionString = version != null
                    ? $"{version.Major}.{version.Minor}.{version.Build}"
                    : "0.0.1";

                // Используем стандартное имя пакета GigaChat.NET-SDK
                _cachedClientId = $"GigaChat.NET-SDK/{versionString}";
            }
            catch
            {
                // В случае ошибки используем значение по умолчанию
                _cachedClientId = "GigaChat.NET-SDK";
            }

            return _cachedClientId;
        }

        /// <summary>
        /// Инициализирует клиент, получая первоначальный токен доступа и настраивая заголовок авторизации.
        /// </summary>
        /// <returns>Задача, представляющая асинхронную операцию инициализации.</returns>
        /// <exception cref="HttpRequestException">Ошибка при получении токена от сервера авторизации.</exception>
        /// <exception cref="InvalidDataException">Получен некорректный ответ от сервера авторизации.</exception>
        /// <remarks>
        /// Метод должен быть вызван перед первым использованием API-методов.
        /// После инициализации токен будет автоматически обновляться при необходимости.
        /// </remarks>
        public async Task InitializeAsync(CancellationToken ct = default)
        {
            var token = await _tokenService.GetValidTokenAsync(ct);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

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
        public async Task<ChatCompletionResponse?> CreateChatCompletionAsync(ChatCompletionRequest query, CancellationToken ct = default)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            var response = await SendWithAutoAuthAsync(
                async c =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/chat/completions")
                    {
                        Content = JsonContent.Create(query)
                    };

                    ApplyXHeaders(request);

                    return await _httpClient.SendAsync(request, c);
                },
                ct
            );

            // Обработка ошибок с детальными сообщениями на русском языке
            await EnsureSuccessWithDetailedErrorAsync(response, "генерация ответа");

            var result = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(cancellationToken: ct);

            // Извлекаем X-заголовки из ответа
            if (result != null)
            {
                result.XHeaders = ExtractXHeaders(response);
            }

            return result;
        }

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
        public async Task<EmbeddingResponse?> CreateEmbeddingsAsync(EmbeddingRequest request, CancellationToken ct = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var response = await SendWithAutoAuthAsync(
                c => _httpClient.PostAsJsonAsync($"{baseUrl}/embeddings", request, ct),
                ct
            );
            await EnsureSuccessWithDetailedErrorAsync(response, "создание эмбеддингов");
            var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(cancellationToken: ct);
            return result;
        }

        /// <summary>
        /// Получает список всех файлов, загруженных в хранилище GigaChat.
        /// </summary>
        /// <param name="ct">Токен отмены операции.</param>
        /// <returns>Коллекция объектов с метаданными всех файлов в хранилище.</returns>
        /// <exception cref="HttpRequestException">Ошибка при выполнении HTTP-запроса к API.</exception>
        /// <exception cref="InvalidDataException">Получен некорректный ответ от сервера.</exception>
        /// <remarks>
        /// Метод автоматически обновляет токен при получении HTTP 401.
        /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/get-files"/></para>
        /// </remarks>
        public async Task<IReadOnlyCollection<GigaChatFile>> GetFilesAsync(CancellationToken ct = default)
        {
            var response = await SendWithAutoAuthAsync(
                async c =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/files");
                    ApplyXHeaders(request);
                    return await _httpClient.SendAsync(request, c);
                },
                ct
            );
            await EnsureSuccessWithDetailedErrorAsync(response, "получениe списка файлов");
            var result = await response.Content.ReadFromJsonAsync<FileListResponse>(ct);
            if (result is null)
            {
                throw new InvalidDataException("Некорректный формат ответа от сервера при получении списка файлов.");
            }

            return result.Files;
        }

        /// <summary>
        /// Получает метаданные файла по его идентификатору.
        /// </summary>
        /// <param name="fileId">Уникальный идентификатор файла в формате UUID.</param>
        /// <param name="ct">Токен отмены операции.</param>
        /// <returns>
        /// Объект с метаданными файла или <c>null</c>, если файл не найден (HTTP 404).
        /// </returns>
        /// <exception cref="HttpRequestException">Ошибка при выполнении HTTP-запроса к API.</exception>
        /// <remarks>
        /// Метод автоматически обновляет токен при получении HTTP 401.
        /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/get-file"/></para>
        /// </remarks>
        public async Task<GigaChatFile?> GetFileAsync(string fileId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(fileId))
                throw new ArgumentException("Идентификатор файла не может быть пустым.", nameof(fileId));

            var response = await SendWithAutoAuthAsync(
                async c =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/files/{fileId}");
                    ApplyXHeaders(request);
                    return await _httpClient.SendAsync(request, c);
                },
                ct
            );

            // HTTP 404 - это ожидаемый результат, когда файл не найден
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            await EnsureSuccessWithDetailedErrorAsync(response, "получениe метаданных файла");

            var result = await response.Content.ReadFromJsonAsync<GigaChatFile>(cancellationToken: ct);
            return result;
        }

        /// <summary>
        /// Скачивает содержимое файла из хранилища GigaChat.
        /// </summary>
        /// <param name="fileId">Уникальный идентификатор файла в формате UUID.</param>
        /// <param name="ct">Токен отмены операции.</param>
        /// <returns>Поток с содержимым файла.</returns>
        /// <exception cref="HttpRequestException">Ошибка при выполнении HTTP-запроса к API.</exception>
        /// <remarks>
        /// Метод автоматически обновляет токен при получении HTTP 401.
        /// Вызывающая сторона ответственна за закрытие возвращаемого потока.
        /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/get-file-content"/></para>
        /// </remarks>
        public async Task<Stream> DownloadFileAsync(string fileId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(fileId))
                throw new ArgumentException("Идентификатор файла не может быть пустым.", nameof(fileId));

            var response = await SendWithAutoAuthAsync(
                async c =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/files/{fileId}/content");
                    ApplyXHeaders(request);
                    return await _httpClient.SendAsync(request, c);
                },
                ct
            );
            await EnsureSuccessWithDetailedErrorAsync(response, "скачиваниe файла");
            return await response.Content.ReadAsStreamAsync();
        }

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
        /// MIME-тип определяется автоматически по расширению файла.
        /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/post-file"/></para>
        /// </remarks>
        public async Task<GigaChatFile> CreateFileAsync(Stream fileStream, string fileName, CancellationToken ct = default)
        {
            if (fileStream == null)
                throw new ArgumentNullException(nameof(fileStream));

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("Имя файла не может быть пустым.", nameof(fileName));

            // Сброс позиции потока в начало, если возможно (для повторного чтения)
            if (fileStream.CanSeek) fileStream.Position = 0;

            // Валидация размера файла согласно ограничениям GigaChat API
            if (fileStream.CanSeek)
            {
                var fileSize = fileStream.Length;
                var category = MimeTypeConstants.GetFileCategory(fileName);
                var maxSize = MimeTypeConstants.GetMaxSizeBytes(category);

                if (fileSize > maxSize)
                {
                    throw new FileSizeExceededException(
                        fileName: fileName,
                        category: category,
                        currentSizeBytes: fileSize,
                        maxSizeBytes: maxSize
                    );
                }
            }

            // Определение MIME-типа по расширению файла
            var contentType = GuessContentType(fileName);

            // Формирование multipart/form-data запроса для загрузки файла
            using var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            content.Add(fileContent, "file", fileName);
            // Поле purpose всегда устанавливается в "general" (требование API)
            content.Add(new StringContent("general", Encoding.UTF8), "purpose");

            var response = await SendWithAutoAuthAsync(
                async c =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/files")
                    {
                        Content = content
                    };
                    ApplyXHeaders(request);
                    return await _httpClient.SendAsync(request, c);
                },
                ct
            );
            await EnsureSuccessWithDetailedErrorAsync(response, "загрузка файла");
            var result = await response.Content.ReadFromJsonAsync<GigaChatFile>(cancellationToken: ct);
            if (result is null)
            {
                throw new InvalidDataException("Некорректный формат ответа от сервера при загрузке файла. " +
                                               "Метаданные файла не были получены.");
            }

            return result;
        }

        /// <summary>
        /// Удаляет файл из хранилища GigaChat.
        /// </summary>
        /// <param name="fileId">Уникальный идентификатор файла в формате UUID.</param>
        /// <param name="ct">Токен отмены операции.</param>
        /// <returns>Результат операции удаления.</returns>
        /// <exception cref="FileNotFoundException">Файл с указанным идентификатором не найден (HTTP 404).</exception>
        /// <exception cref="HttpRequestException">Ошибка при выполнении HTTP-запроса к API.</exception>
        /// <remarks>
        /// Метод автоматически обновляет токен при получении HTTP 401.
        /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/post-file-delete"/></para>
        /// </remarks>
        public async Task<FileDeleteResponse> DeleteFileAsync(string fileId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(fileId))
                throw new ArgumentException("Идентификатор файла не может быть пустым.", nameof(fileId));

            var response = await SendWithAutoAuthAsync(
                async c =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/files/{fileId}/delete")
                    {
                        Content = JsonContent.Create(new { })
                    };
                    ApplyXHeaders(request);
                    return await _httpClient.SendAsync(request, c);
                },
                ct
            );
            // Специальная обработка HTTP 404: файл не найден в хранилище
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new FileNotFoundException($"Файл с идентификатором {fileId} не найден в хранилище GigaChat.");
            }

            await EnsureSuccessWithDetailedErrorAsync(response, "удаление файла");
            var result = await response.Content.ReadFromJsonAsync<FileDeleteResponse>(cancellationToken: ct);
            return result ?? throw new InvalidOperationException("Не удалось десериализовать ответ API при удалении файла.");
        }

        /// <summary>
        /// Получает изображение из хранилища GigaChat в виде массива байтов.
        /// </summary>
        /// <param name="fileId">Уникальный идентификатор изображения в формате UUID.</param>
        /// <param name="ct">Токен отмены операции.</param>
        /// <returns>Массив байтов с содержимым изображения (обычно в формате JPEG).</returns>
        /// <exception cref="ArgumentException">Параметр <paramref name="fileId"/> является пустым или содержит только пробелы.</exception>
        /// <exception cref="HttpRequestException">Ошибка при выполнении HTTP-запроса к API.</exception>
        /// <remarks>
        /// Метод автоматически обновляет токен при получении HTTP 401.
        /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/get-file-content"/></para>
        /// </remarks>
        public async Task<byte[]?> DownloadImageAsync(string fileId, CancellationToken ct = default)
        {

            if (string.IsNullOrWhiteSpace(fileId))
                throw new ArgumentException("Идентификатор файла не может быть пустым.", nameof(fileId));

            var response = await SendWithAutoAuthAsync(
                async c =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/files/{fileId}/content");
                    ApplyXHeaders(request);
                    return await _httpClient.SendAsync(request, c);
                },
                ct
            );
            await EnsureSuccessWithDetailedErrorAsync(response, "получение содержимого изображения");
            var result = await response.Content.ReadAsByteArrayAsync();
            return result;
        }

        /// <summary>
        /// Извлекает идентификатор файла из содержимого сообщения GigaChat.
        /// </summary>
        /// <param name="messageContent">Содержимое сообщения, которое может содержать HTML-тег изображения.</param>
        /// <returns>
        /// Идентификатор файла (путь из атрибута <c>src</c> тега <c>&lt;img&gt;</c>) или <c>null</c>,
        /// если тег изображения не найден.
        /// </returns>
        /// <exception cref="ArgumentException">Параметр <paramref name="messageContent"/> является пустым или содержит только пробелы.</exception>
        /// <remarks>
        /// Метод использует регулярное выражение для поиска шаблона <c>&lt;img src="..."&gt;</c> в тексте сообщения.
        /// Полезен для извлечения идентификаторов сгенерированных изображений из ответов модели.
        /// </remarks>
        public string? GetFileId(string messageContent)
        {
            if (string.IsNullOrWhiteSpace(messageContent))
                throw new ArgumentException("Содержимое сообщения не может быть пустым.", nameof(messageContent));

            // Поиск HTML-тега изображения в ответе модели
            // Модель GigaChat включает сгенерированные изображения в формате <img src="...">
            var pattern = "<img\\s+src=\"(.*?)\"";
            var match = Regex.Match(messageContent, pattern);
            return match.Success ? match.Groups[1].Value : null;
        }

        /// <summary>
        /// Получает список всех доступных моделей GigaChat.
        /// </summary>
        /// <param name="ct">Токен отмены операции.</param>
        /// <returns>Коллекция объектов с метаданными доступных моделей.</returns>
        /// <exception cref="HttpRequestException">Ошибка при выполнении HTTP-запроса к API.</exception>
        /// <exception cref="InvalidDataException">Получен некорректный ответ от сервера.</exception>
        /// <remarks>
        /// Возвращает информацию о всех доступных моделях, включая модели для генерации текста,
        /// создания эмбеддингов и проверки ИИ-контента.
        /// Метод автоматически обновляет токен при получении HTTP 401.
        /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/get-models"/></para>
        /// </remarks>
        public async Task<IReadOnlyCollection<Model>> GetModelsAsync(CancellationToken ct = default)
        {
            var response = await SendWithAutoAuthAsync(
                c => _httpClient.GetAsync($"{baseUrl}/models", ct),
                ct
            );
            await EnsureSuccessWithDetailedErrorAsync(response, "получение списка моделей");

            var result = await response.Content.ReadFromJsonAsync<ModelListResponse>(cancellationToken: ct);
            if (result?.Data == null)
            {
                throw new InvalidDataException("Некорректный формат ответа от сервера при получении списка моделей.");
            }

            return result.Data;
        }

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
        /// Метод автоматически обновляет токен при получении HTTP 401.
        /// При HTTP 404 (модель не найдена) возвращает <c>null</c>.
        /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/get-model"/></para>
        /// </remarks>
        public async Task<Model?> GetModelAsync(string modelId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(modelId))
                throw new ArgumentException("Идентификатор модели не может быть пустым.", nameof(modelId));

            var response = await SendWithAutoAuthAsync(
                c => _httpClient.GetAsync($"{baseUrl}/models/{modelId}", ct),
                ct
            );

            // Специальная обработка HTTP 404: модель не найдена
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            await EnsureSuccessWithDetailedErrorAsync(response, "получение информации о модели");

            var result = await response.Content.ReadFromJsonAsync<Model>(cancellationToken: ct);
            return result;
        }

        /// <summary>
        /// Подсчитывает количество токенов в текстовых строках.
        /// </summary>
        /// <param name="request">Запрос с массивом строк и названием модели для подсчета.</param>
        /// <param name="ct">Токен отмены операции.</param>
        /// <returns>Объект с информацией о количестве токенов для каждой строки.</returns>
        /// <exception cref="ArgumentException">Массив строк пустой или модель не указана.</exception>
        /// <exception cref="HttpRequestException">Ошибка при выполнении HTTP-запроса к API.</exception>
        /// <exception cref="InvalidDataException">Получен некорректный ответ от сервера.</exception>
        /// <remarks>
        /// Позволяет заранее оценить стоимость запроса перед отправкой к модели.
        /// Индекс элемента в ответе соответствует индексу строки в запросе.
        /// Метод автоматически обновляет токен при получении HTTP 401.
        /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/post-tokens-count"/></para>
        /// </remarks>
        public async Task<TokensCountResponse> CountTokensAsync(TokensCountRequest request, CancellationToken ct = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.Model))
                throw new ArgumentException("Модель не может быть пустой.", nameof(request.Model));

            if (request.Input == null || request.Input.Count == 0)
                throw new ArgumentException("Массив строк для подсчета токенов не может быть пустым.", nameof(request.Input));

            var response = await SendWithAutoAuthAsync(
                c => _httpClient.PostAsJsonAsync($"{baseUrl}/tokens/count", request, ct),
                ct
            );
            await EnsureSuccessWithDetailedErrorAsync(response, "подсчета токенов");

            var result = await response.Content.ReadFromJsonAsync<List<TokenCountItem>>(cancellationToken: ct);
            if (result == null)
            {
                throw new InvalidDataException("Некорректный формат ответа от сервера при подсчете токенов.");
            }

            return new TokensCountResponse { Tokens = result };
        }

        /// <summary>
        /// Получает остаток токенов для каждой доступной модели.
        /// </summary>
        /// <param name="ct">Токен отмены операции.</param>
        /// <returns>Объект с информацией об остатках токенов по моделям.</returns>
        /// <exception cref="HttpRequestException">Ошибка при выполнении HTTP-запроса к API.</exception>
        /// <exception cref="InvalidDataException">Получен некорректный ответ от сервера.</exception>
        /// <remarks>
        /// Метод доступен только при покупке пакетов токенов.
        /// Если вы оплачиваете работу с API по схеме pay-as-you-go, запрос вернет ошибку 403 Permission Denied.
        /// Метод автоматически обновляет токен при получении HTTP 401.
        /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/get-balance"/></para>
        /// </remarks>
        public async Task<BalanceResponse> GetBalanceAsync(CancellationToken ct = default)
        {
            var response = await SendWithAutoAuthAsync(
                c => _httpClient.GetAsync($"{baseUrl}/balance", ct),
                ct
            );
            await EnsureSuccessWithDetailedErrorAsync(response, "получения баланса токенов");

            var result = await response.Content.ReadFromJsonAsync<BalanceResponse>(cancellationToken: ct);
            if (result == null)
            {
                throw new InvalidDataException("Некорректный формат ответа от сервера при получении баланса.");
            }

            return result;
        }

        /// <summary>
        /// Проверяет текст на наличие AI-генерированного контента.
        /// </summary>
        /// <param name="request">Запрос с текстом и моделью для проверки.</param>
        /// <param name="ct">Токен отмены операции.</param>
        /// <returns>Результат проверки с категорией текста и найденными AI-фрагментами.</returns>
        /// <exception cref="ArgumentException">Текст пустой или модель не указана.</exception>
        /// <exception cref="HttpRequestException">Ошибка при выполнении HTTP-запроса к API.</exception>
        /// <exception cref="InvalidDataException">Получен некорректный ответ от сервера.</exception>
        /// <remarks>
        /// Проверка доступна только для текстов на русском языке (минимум 20 слов).
        /// Метод доступен только для юридических лиц, работающих по схеме оплаты pay-as-you-go.
        /// Метод автоматически обновляет токен при получении HTTP 401.
        /// <para>Доступные модели:</para>
        /// <list type="bullet">
        /// <item><c>GigaCheckClassification</c> — определяет, написан ли текст человеком или AI (ai/human)</item>
        /// <item><c>GigaCheckDetection</c> — дополнительно определяет смешанный контент (mixed)</item>
        /// </list>
        /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/post-ai-check"/></para>
        /// </remarks>
        public async Task<AiCheckResponse> CheckAiContentAsync(AiCheckRequest request, CancellationToken ct = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.Model))
                throw new ArgumentException("Модель не может быть пустой.", nameof(request.Model));

            if (string.IsNullOrWhiteSpace(request.Input))
                throw new ArgumentException("Текст для проверки не может быть пустым.", nameof(request.Input));

            var response = await SendWithAutoAuthAsync(
                c => _httpClient.PostAsJsonAsync($"{baseUrl}/ai/check", request, ct),
                ct
            );
            await EnsureSuccessWithDetailedErrorAsync(response, "проверки AI-контента");

            var result = await response.Content.ReadFromJsonAsync<AiCheckResponse>(cancellationToken: ct);
            if (result == null)
            {
                throw new InvalidDataException("Некорректный формат ответа от сервера при проверке AI-контента.");
            }

            return result;
        }

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
        /// Для загрузки сгенерированного изображения используйте методы <c>DownloadImageAsync</c> или <c>DownloadFileAsync</c> с полученным FileId.
        /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/guides/images-generation"/></para>
        /// </remarks>
        public async Task<ImageGenerationResponse?> CreateImageAsync(string prompt, string model = "GigaChat", string? systemPrompt = null, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                throw new ArgumentException("Промпт для генерации изображения не может быть пустым.", nameof(prompt));

            var query = new ChatCompletionRequest
            {
                Model = model,
                Messages = new List<ChatMessage>(),
                FunctionCall = "auto"
            };

            // Добавление системного промпта для стилизации, если указан
            if (!string.IsNullOrWhiteSpace(systemPrompt))
            {
                query.Messages.Add(new ChatMessage("system", systemPrompt));
            }

            // Добавление пользовательского запроса на генерацию
            query.Messages.Add(new ChatMessage("user", prompt));

            var response = await SendWithAutoAuthAsync(
                c => _httpClient.PostAsJsonAsync($"{baseUrl}/chat/completions", query, ct),
                ct
            );

            await EnsureSuccessWithDetailedErrorAsync(response, "генерация изображения");

            var chatResponse = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(cancellationToken: ct);

            if (chatResponse == null || chatResponse.Choices == null || chatResponse.Choices.Count == 0)
                return null;

            var messageContent = chatResponse.Choices[0]?.Message?.Content;
            if (string.IsNullOrWhiteSpace(messageContent))
                return null;

            var fileId = GetFileId(messageContent);
            if (string.IsNullOrWhiteSpace(fileId))
                throw new InvalidOperationException("Не удалось извлечь идентификатор файла из ответа модели. Возможно, изображение не было сгенерировано.");

            return new ImageGenerationResponse(
                fileId: fileId,
                prompt: prompt,
                model: chatResponse.Model,
                created: chatResponse.Created
            );
        }

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
        public async Task<string> RecognizeImageAsync(string fileId, string prompt = "Опиши, что изображено на фото", string model = "GigaChat-2-Max", CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(fileId))
                throw new ArgumentException("Идентификатор файла не может быть пустым.", nameof(fileId));

            if (string.IsNullOrWhiteSpace(prompt))
                throw new ArgumentException("Промпт для распознавания не может быть пустым.", nameof(prompt));

            var userMessage = new ChatMessage("user", prompt)
            {
                Attachments = new List<string> { fileId }
            };

            var query = new ChatCompletionRequest
            {
                Model = model,
                Messages = new List<ChatMessage> { userMessage },
                FunctionCall = "auto"
            };

            var response = await SendWithAutoAuthAsync(
                c => _httpClient.PostAsJsonAsync($"{baseUrl}/chat/completions", query, ct),
                ct
            );

            await EnsureSuccessWithDetailedErrorAsync(response, "распознавание изображения");

            var chatResponse = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(cancellationToken: ct);

            return chatResponse?.Choices?[0]?.Message?.Content ?? string.Empty;
        }

        /// <summary>
        /// Применяет дополнительные X-заголовки к HTTP-запросу.
        /// </summary>
        /// <param name="request">HTTP-запрос для добавления заголовков.</param>
        private void ApplyXHeaders(HttpRequestMessage request)
        {
            if (RequestHeaders == null)
                return;

            if (!string.IsNullOrWhiteSpace(RequestHeaders.XSessionId))
            {
                request.Headers.TryAddWithoutValidation("X-Session-ID", RequestHeaders.XSessionId);
            }

            if (!string.IsNullOrWhiteSpace(RequestHeaders.XRequestId))
            {
                request.Headers.TryAddWithoutValidation("X-Request-ID", RequestHeaders.XRequestId);
            }

            if (!string.IsNullOrWhiteSpace(RequestHeaders.XClientId))
            {
                request.Headers.TryAddWithoutValidation("X-Client-ID", RequestHeaders.XClientId);
            }
        }

        /// <summary>
        /// Извлекает X-заголовки из ответа HTTP.
        /// </summary>
        /// <param name="response">HTTP-ответ для извлечения заголовков.</param>
        /// <returns>Объект с извлеченными заголовками или null, если заголовки отсутствуют.</returns>
        private static XRequestHeaders? ExtractXHeaders(HttpResponseMessage response)
        {
            var headers = new XRequestHeaders();
            bool hasAnyHeader = false;

            if (response.Headers.TryGetValues("X-Request-ID", out var requestIdValues))
            {
                headers.XRequestId = requestIdValues.FirstOrDefault();
                hasAnyHeader = true;
            }

            if (response.Headers.TryGetValues("X-Session-ID", out var sessionIdValues))
            {
                headers.XSessionId = sessionIdValues.FirstOrDefault();
                hasAnyHeader = true;
            }

            if (response.Headers.TryGetValues("X-Client-ID", out var clientIdValues))
            {
                headers.XClientId = clientIdValues.FirstOrDefault();
                hasAnyHeader = true;
            }

            return hasAnyHeader ? headers : null;
        }

        /// <summary>
        /// Внутренний метод для отправки запросов с автоматической повторной авторизацией при HTTP 401.
        /// </summary>
        /// <remarks>
        /// Реализует паттерн retry с одной попыткой:
        /// 1. Выполняет запрос
        /// 2. Если получен HTTP 401 (Unauthorized), обновляет токен и повторяет запрос один раз
        /// 3. Если статус не 401, возвращает ответ как есть
        /// </remarks>
        private async Task<HttpResponseMessage> SendWithAutoAuthAsync(
            Func<CancellationToken, Task<HttpResponseMessage>> send,
            CancellationToken ct)
        {
            // Первая попытка выполнения запроса
            var resp = await send(ct);
            if (resp.StatusCode != HttpStatusCode.Unauthorized)
                return resp;

            // HTTP 401: токен истек или невалиден
            // Освобождаем ресурсы первого ответа для предотвращения утечки сокетов
            resp.Dispose();

            // Получаем новый валидный токен (может включать обновление)
            var token = await _tokenService.GetValidTokenAsync(ct);
            // Обновляем заголовок авторизации для всех последующих запросов
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Повторная попытка запроса с обновленным токеном (только один retry)
            var retry = await send(ct);
            return retry;
        }

        /// <summary>
        /// Проверяет успешность HTTP-ответа и выбрасывает типизированное исключение при ошибке.
        /// </summary>
        /// <param name="response">HTTP-ответ для проверки.</param>
        /// <param name="operationName">Название операции для сообщения об ошибке.</param>
        /// <remarks>
        /// Формирует детальные сообщения об ошибках на русском языке и выбрасывает
        /// соответствующие типизированные исключения из пространства имен GigaChatSDK.Exceptions.
        /// </remarks>
        private static async Task EnsureSuccessWithDetailedErrorAsync(HttpResponseMessage response, string operationName)
        {
            if (response.IsSuccessStatusCode)
                return;

            var errorBody = await response.Content.ReadAsStringAsync();
            var statusCode = (int)response.StatusCode;

            switch (statusCode)
            {
                case 400:
                    throw new GigaChatException(
                        $"Ошибка в параметрах запроса {operationName}. " +
                        $"Проверьте корректность переданных данных. Детали: {errorBody}",
                        400, errorBody);

                case 401:
                    throw new AuthenticationException(
                        $"Ошибка авторизации при {operationName}. " +
                        $"Токен доступа истек (действует 30 минут) или некорректен. " +
                        $"Токен будет автоматически обновлен. Детали: {errorBody}",
                        errorBody);

                case 402:
                    throw new InsufficientBalanceException(
                        $"Закончились токены модели при {operationName}. " +
                        $"Пополните баланс в личном кабинете. Детали: {errorBody}",
                        errorBody);

                case 403:
                    throw new AccessForbiddenException(
                        $"Нет доступа к ресурсу при {operationName}. " +
                        $"Проверьте права доступа и тип подписки. Детали: {errorBody}",
                        errorBody);

                case 404:
                    throw new ResourceNotFoundException(
                        $"Ресурс не найден при {operationName}. " +
                        $"Проверьте корректность идентификатора. Детали: {errorBody}",
                        null, errorBody);

                case 413:
                    throw new RequestTooLargeException(
                        $"Превышен максимальный размер входных данных при {operationName}. " +
                        $"Уменьшите размер промпта. Используйте метод /tokens/count для оценки. Детали: {errorBody}",
                        errorBody);

                case 422:
                    throw new RequestValidationException(
                        $"Ошибка валидации параметров при {operationName}. " +
                        $"Проверьте порядок сообщений и значения параметров. " +
                        $"Системный промпт должен быть первым. Детали: {errorBody}",
                        errorBody);

                case 429:
                    throw new RateLimitExceededException(
                        $"Превышен лимит запросов при {operationName}. " +
                        $"Физ. лицам доступен 1 поток, ИП и юрлицам — 10. Детали: {errorBody}",
                        null, errorBody);

                case 500:
                    throw new ServerErrorException(
                        $"Внутренняя ошибка сервиса GigaChat при {operationName}. " +
                        $"Обратитесь в поддержку: gigachat@sberbank.ru. Детали: {errorBody}",
                        errorBody);

                default:
                    throw new GigaChatException(
                        $"Ошибка при операции \"{operationName}\" (HTTP {statusCode}). Детали: {errorBody}",
                        statusCode, errorBody);
            }
        }

        /// <summary>
        /// Определяет MIME-тип файла по его расширению.
        /// </summary>
        /// <remarks>
        /// Использует словарь <see cref="MimeTypeConstants.GigaChatMime"/> для сопоставления расширений и MIME-типов.
        /// Возвращает "application/octet-stream" для неизвестных расширений или файлов без расширения.
        /// </remarks>
        private static string GuessContentType(string fileName)
        {
            var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(ext))
                return "application/octet-stream";

            // Поиск MIME-типа в словаре поддерживаемых расширений
            return MimeTypeConstants.GigaChatMime.TryGetValue(ext, out var mime)
                ? mime
                : "application/octet-stream";
        }
    }
}
