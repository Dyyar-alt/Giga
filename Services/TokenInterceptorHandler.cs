namespace Giga.Services
{
    public class TokenInterceptorHandler : DelegatingHandler
    {
        private string _cachedAccessToken = string.Empty;
        private readonly object _lock = new object();

        public TokenInterceptorHandler(HttpMessageHandler innerHandler) : base(innerHandler)
        {
        }

        public string? GetAccessToken()
        {
            lock (_lock)
            {
                return string.IsNullOrEmpty(_cachedAccessToken) ? null : _cachedAccessToken;
            }
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            
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
                                System.Diagnostics.Debug.WriteLine($"Токен перехвачен: {token.Substring(0, Math.Min(20, token.Length))}...");
                            }
                        }
                    }
                }
            }

                       return await base.SendAsync(request, cancellationToken);
        }
    }
}
