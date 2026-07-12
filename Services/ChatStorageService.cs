using Giga.Models; // Убедитесь, что здесь указано ваше пространство имен для моделей
using System.Text.Json;

namespace Giga.Services
{
    // Сервис для сохранения и загрузки сессий чата в локальное хранилище.
    public class ChatStorageService
    {
        private readonly string _storagePath;


        // Инициализирует новый экземпляр сервиса.
        // Создает папку "ChatHistory" в директории данных приложения, если она не существует.
        public ChatStorageService()
        {

            _storagePath = Path.Combine(FileSystem.AppDataDirectory, "ChatHistory");

            //безопасно создаём директорию, если ее нет,
            // или ничего не делаем, если она уже существует.
            Directory.CreateDirectory(_storagePath);
        }


        //Асинхронно сохраняем сессию чата в файл формата JSON.
        /// Имя файла генерируется на основе ID сессии и времени начала.

        public async Task SaveChatAsync(ChatSession session)
        {
            try
            {
                // Генерируем уникальное имя файла, чтобы избежать перезаписи старых чатов.
                var fileName = $"{session.Id}_{session.StartTime:yyyyMMdd_HHmmss}.json";
                var filePath = Path.Combine(_storagePath, fileName);

                // Сериализуем объект ChatSession в форматированный JSON.
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


        // Асинхронно загружает сессию чата из указанного файла JSON.

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


        // Получаем список всех сохраненных сессий чата.

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
                        // Синхронно читаем файл.
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