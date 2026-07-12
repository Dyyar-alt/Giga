using GigaChatSDK;
using GigaChatSDK.Models.ApiRequest;
using Microsoft.Extensions.Logging;
using Giga.Models;

#if ANDROID
using Giga.Platforms.Android;
#endif

namespace Giga.Services
{
    public class GigaChatService
    {
        private GigaChat? _client;
        private readonly string _authKey;
        private readonly GigaChatScope _scope;
        private bool _isInitialized = false;

        public bool IsInitialized => _isInitialized;

        public GigaChatService(string authKey, GigaChatScope scope = GigaChatScope.GIGACHAT_API_PERS)
        {
            _authKey = authKey;
            _scope = scope;
        }

        public async Task InitializeAsync()
        {
            if (string.IsNullOrEmpty(_authKey))
                throw new InvalidOperationException("Auth key is empty");

            try
            {
                //Сначала отключаем SSL на уровне приложения
                System.Net.ServicePointManager.ServerCertificateValidationCallback =
                    (sender, certificate, chain, sslPolicyErrors) => true;
                System.Net.ServicePointManager.SecurityProtocol =
                    System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Tls13;

#if ANDROID
                // Для Android используем кастомный Handler
                try
                {
                    var handler = new AndroidHttpClientHandler();
                    System.Diagnostics.Debug.WriteLine("AndroidHttpClientHandler создан");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"AndroidHttpClientHandler ошибка: {ex.Message}");
                }
#endif

                using var loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder.SetMinimumLevel(LogLevel.Information);
                });
                var logger = loggerFactory.CreateLogger<GigaChat>();

                var builder = new GigaChatBuilder()
                    .WithAuthKey(_authKey)
                    .WithScope(_scope)
                    .WithLogger(logger);

                //Игнорируем TLS
                try
                {
                    builder.IgnoreTLS();
                    System.Diagnostics.Debug.WriteLine("IgnoreTLS включен");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"IgnoreTLS ошибка: {ex.Message}");
                }

                _client = builder.Build();

                await _client.InitializeAsync();
                _isInitialized = true;

                System.Diagnostics.Debug.WriteLine("GigaChatService инициализирован успешно");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации GigaChatService: {ex.Message}");
                throw;
            }
        }

        // Асинхронная отправка сообщения
        public async Task<string> SendMessageAsync(string message)
        {
            if (!_isInitialized || _client == null)
            {
                System.Diagnostics.Debug.WriteLine("Client not initialized, попытка переинициализации...");
                await InitializeAsync();

                if (!_isInitialized || _client == null)
                    throw new InvalidOperationException("Client not initialized");
            }

            try
            {
                var response = await _client.ChatAsync(message);
                return response.GetContent();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка отправки сообщения: {ex.Message}");

                if (ex.Message.Contains("certificate") || ex.Message.Contains("SSL") || ex.Message.Contains("TLS") || ex.Message.Contains("Authentication"))
                {
                    System.Diagnostics.Debug.WriteLine("Ошибка сертификата, пробуем переинициализацию...");
                    _isInitialized = false;
                    await InitializeAsync();

                    if (_isInitialized && _client != null)
                    {
                        var response = await _client.ChatAsync(message);
                        return response.GetContent();
                    }
                }

                throw;
            }
        }
        //Асинхронное получение изображения
        public async Task<byte[]?> DownloadImageAsync(string fileId)
        {
            if (!_isInitialized || _client == null)
            {
                System.Diagnostics.Debug.WriteLine("Client not initialized, попытка переинициализации...");
                await InitializeAsync();

                if (!_isInitialized || _client == null)
                    throw new InvalidOperationException("Client not initialized");
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"Скачивание через SDK: {fileId}");
                var imageData = await _client.DownloadImageAsync(fileId);
                System.Diagnostics.Debug.WriteLine($"Загружено через SDK: {imageData?.Length ?? 0} байт");
                return imageData;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка скачивания через SDK: {ex.Message}");

                if (ex.Message.Contains("certificate") || ex.Message.Contains("SSL") || ex.Message.Contains("TLS") || ex.Message.Contains("Authentication"))
                {
                    System.Diagnostics.Debug.WriteLine("Ошибка сертификата при скачивании, пробуем переинициализацию...");
                    _isInitialized = false;
                    await InitializeAsync();

                    if (_isInitialized && _client != null)
                    {
                        var imageData = await _client.DownloadImageAsync(fileId);
                        System.Diagnostics.Debug.WriteLine($"Загружено после переинициализации: {imageData?.Length ?? 0} байт");
                        return imageData;
                    }
                }

                return null;
            }
        }
        //Получение и подсчет токенов
        public async Task<TokenInfo?> CountTokensAsync(string userMessage, string botResponse)
        {
            try
            {
                if (_client == null)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Клиент не инициализирован, подсчёт токенов пропущен");
                    return null;
                }

                var tokenInfo = new TokenInfo();

                var userRequest = new TokensCountRequest(
                    new List<string> { userMessage },
                    "GigaChat"
                );
                var userTokensResponse = await _client.CountTokensAsync(userRequest);

                var botRequest = new TokensCountRequest(
                    new List<string> { botResponse },
                    "GigaChat"
                );
                var botTokensResponse = await _client.CountTokensAsync(botRequest);

                if (userTokensResponse.Tokens?.Any() == true && botTokensResponse.Tokens?.Any() == true)
                {
                    var userTokenInfo = userTokensResponse.Tokens.First();
                    var botTokenInfo = botTokensResponse.Tokens.First();

                    tokenInfo.UserTokens = userTokenInfo.Tokens;
                    tokenInfo.BotTokens = botTokenInfo.Tokens;
                    tokenInfo.TotalTokens = userTokenInfo.Tokens + botTokenInfo.Tokens;
                    tokenInfo.TotalCharacters = userTokenInfo.Characters + botTokenInfo.Characters;
                    tokenInfo.UserCharacters = userTokenInfo.Characters;
                    tokenInfo.BotCharacters = botTokenInfo.Characters;

                    return tokenInfo;
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка подсчета токенов: {ex.Message}");
                return null; // Не выбрасываем исключение, а возвращаем null
            }
        }
    }
}