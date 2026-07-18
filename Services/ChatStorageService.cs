using Giga.Models;
using System.Text.Json;

namespace Giga.Services
{
    // Сервис для сохранения и загрузки сессий чата в локальное хранилище.
    public class ChatStorageService
    {
        private readonly string _storagePath;


        // Создает папку "ChatHistory" в директории данных приложения, если она не существует.
        public ChatStorageService()
        {

            _storagePath = Path.Combine(FileSystem.AppDataDirectory, "ChatHistory");

              Directory.CreateDirectory(_storagePath);
        }


        //Сохраняет сессию чата в файл формата JSON.
        // Имя файла генерируется на основе ID сессии и времени начала.

        public async Task SaveChatAsync(ChatSession session)
        {
            try
            {
                // Генерируем уникальное имя файла, чтобы избежать перезаписи старых чатов.
                var fileName = $"{session.Id}_{session.StartTime:yyyyMMdd_HHmmss}.json";
                var filePath = Path.Combine(_storagePath, fileName);

              
                var json = JsonSerializer.Serialize(session, new JsonSerializerOptions { WriteIndented = true });

                
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении чата: {ex.Message}");
                throw;
            }
        }


       
        public async Task<ChatSession> LoadChatAsync(string filePath)
        {
            try
            {
                var json = await File.ReadAllTextAsync(filePath);

               return JsonSerializer.Deserialize<ChatSession>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке чата из файла {filePath}: {ex.Message}");
                throw;
            }
        }


     
        public List<ChatSession> GetAllSessions()
        {
            var sessions = new List<ChatSession>();

            try
            {
               
                var files = Directory.GetFiles(_storagePath, "*.json");

                foreach (var file in files)
                {
                    try
                    {
                       
                        var json = File.ReadAllText(file);
                        var session = JsonSerializer.Deserialize<ChatSession>(json);

                        if (session != null)
                        {
                            sessions.Add(session);
                        }
                    }
                    catch (Exception ex)
                    {
                        
                        Console.WriteLine($"Ошибка загрузки {file}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка доступа к папке {_storagePath}: {ex.Message}");
            }

            return sessions.OrderByDescending(s => s.StartTime).ToList();
        }
    }
}
