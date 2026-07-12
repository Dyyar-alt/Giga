namespace Giga.Services;

public interface IApiKeyService
{
    Task<string> GetApiKeyAsync();
    Task SaveApiKeyAsync(string apiKey);
    Task<bool> HasKeyAsync();
}

public class ApiKeyService : IApiKeyService
{
    private const string KeyStorageKey = "giga_api_key";

    public Task<string> GetApiKeyAsync()
    {
        try
        {
            var key = Preferences.Get(KeyStorageKey, string.Empty);
            System.Diagnostics.Debug.WriteLine($"Ключ из Preferences: {(string.IsNullOrEmpty(key) ? "НЕТ" : $"ЕСТЬ ({key.Length} символов)")}");
            return Task.FromResult(key);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка чтения Preferences: {ex.Message}");
            return Task.FromResult(string.Empty);
        }
    }

    public Task SaveApiKeyAsync(string apiKey)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                System.Diagnostics.Debug.WriteLine("Попытка сохранить пустой ключ");
                // Удаляем ключ, если передана пустая строка
                Preferences.Remove(KeyStorageKey);
                System.Diagnostics.Debug.WriteLine("Ключ удален из хранилища");
                return Task.CompletedTask;
            }

            Preferences.Set(KeyStorageKey, apiKey.Trim());
            System.Diagnostics.Debug.WriteLine($"Ключ сохранён в Preferences (длина: {apiKey.Trim().Length} символов)");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка сохранения в Preferences: {ex.Message}");
            throw;
        }
    }

    public Task<bool> HasKeyAsync()
    {
        try
        {
            var key = Preferences.Get(KeyStorageKey, string.Empty);
            return Task.FromResult(!string.IsNullOrEmpty(key));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка проверки ключа: {ex.Message}");
            return Task.FromResult(false);
        }
    }
}