using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Giga.Services
{
    public class GlobalTokenHandler : HttpClientHandler
    {
        private static string _cachedAccessToken = string.Empty;
        private static readonly object _lock = new object();

        public static string? GetAccessToken()
        {
            lock (_lock)
            {
                return string.IsNullOrEmpty(_cachedAccessToken) ? null : _cachedAccessToken;
            }
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // ✅ Перехватываем запрос и сохраняем токен из заголовка
            if (request.Headers.Contains("Authorization"))
            {
                var authHeader = request.Headers.GetValues("Authorization").FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    var token = authHeader.Substring("Bearer ".Length);
                    if (!string.IsNullOrEmpty(token) && token.Length > 10)
                    {
                        lock (_lock)
                        {
                            if (_cachedAccessToken != token)
                            {
                                _cachedAccessToken = token;
                                System.Diagnostics.Debug.WriteLine($"✅ Токен перехвачен (глобально): {token.Substring(0, Math.Min(20, token.Length))}...");
                            }
                        }
                    }
                }
            }

            // ✅ Перехватываем ответ и сохраняем токен, если он есть в ответе
            var response = await base.SendAsync(request, cancellationToken);

            // Некоторые API возвращают токен в заголовке ответа
            if (response.Headers.Contains("Authorization"))
            {
                var authHeader = response.Headers.GetValues("Authorization").FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    var token = authHeader.Substring("Bearer ".Length);
                    if (!string.IsNullOrEmpty(token) && token.Length > 10)
                    {
                        lock (_lock)
                        {
                            if (_cachedAccessToken != token)
                            {
                                _cachedAccessToken = token;
                                System.Diagnostics.Debug.WriteLine($"✅ Токен перехвачен из ответа: {token.Substring(0, Math.Min(20, token.Length))}...");
                            }
                        }
                    }
                }
            }

            return response;
        }
    }
}