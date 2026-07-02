using Giga.Models; // Убедитесь, что здесь указано ваше пространство имен для моделей
using System.Text.Json;

namespace Giga.Services
{
    /// <summary>
    /// Сервис для сохранения и загрузки сессий чата в локальное хранилище.
    /// </summary>
    public class ChatStorageService
    {
        private readonly string _storagePath;

        /// <summary>
        /// Инициализирует новый экземпляр сервиса.
        /// Создает папку "ChatHistory" в директории данных приложения, если она не существует.
        /// </summary>
        public ChatStorageService()
        {
            // Используем кроссплатформенное AppDataDirectory вместо Application.StartupPath
            _storagePath = Path.Combine(FileSystem.AppDataDirectory, "ChatHistory");

            // Directory.CreateDirectory безопасно создает директорию, если ее нет,
            // или ничего не делает, если она уже существует.
            Directory.CreateDirectory(_storagePath);
        }

        /// <summary>
        /// Асинхронно сохраняет сессию чата в файл формата JSON.
        /// Имя файла генерируется на основе ID сессии и времени начала.
        /// </summary>
        /// <param name="session">Объект сессии чата для сохранения.</param>
        public async Task SaveChatAsync(ChatSession session)
        {
            try
            {
                // Генерируем уникальное имя файла, чтобы избежать перезаписи старых чатов.
                var fileName = $"{session.Id}_{session.StartTime:yyyyMMdd_HHmmss}.json";
                var filePath = Path.Combine(_storagePath, fileName);

                // Сериализуем объект ChatSession в форматированный (pretty-print) JSON.
                var json = JsonSerializer.Serialize(session, new JsonSerializerOptions { WriteIndented = true });

                // Записываем строку JSON в файл асинхронно.
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении чата: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Асинхронно загружает сессию чата из указанного файла JSON.
        /// </summary>
        /// <param name="filePath">Полный путь к файлу .json.</param>
        /// <returns>Десериализованный объект ChatSession.</returns>
        public async Task<ChatSession> LoadChatAsync(string filePath)
        {
            try
            {
                // Читаем содержимое файла асинхронно.
                var json = await File.ReadAllTextAsync(filePath);

                // Десериализуем JSON обратно в объект ChatSession.
                return JsonSerializer.Deserialize<ChatSession>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке чата из файла {filePath}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Получает список всех сохраненных сессий чата.
        /// </summary>
        /// <returns>Список объектов ChatSession, отсортированных по дате начала (новые сверху).</returns>
        public List<ChatSession> GetAllSessions()
        {
            var sessions = new List<ChatSession>();

            try
            {
                // Находим все файлы .json в папке хранения.
                var files = Directory.GetFiles(_storagePath, "*.json");

                foreach (var file in files)
                {
                    try
                    {
                        // Синхронно читаем файл. Для получения списка это допустимо.
                        var json = File.ReadAllText(file);
                        var session = JsonSerializer.Deserialize<ChatSession>(json);

                        if (session != null)
                        {
                            sessions.Add(session);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Если один файл поврежден, пропускаем его и продолжаем работу с остальными.
                        Console.WriteLine($"Ошибка загрузки {file}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка доступа к папке {_storagePath}: {ex.Message}");
            }

            // Сортируем сессии так, чтобы самые новые были в начале списка.
            return sessions.OrderByDescending(s => s.StartTime).ToList();
        }
    }
}