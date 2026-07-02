using System.Text.Json.Serialization;
using GigaChatSDK.Models.Functions;

namespace GigaChatSDK.Models.Chat
{
    /// <summary>
    /// Представляет одно сообщение в диалоге с GigaChat.
    /// </summary>
    /// <remarks>
    /// Содержит роль отправителя, текст сообщения и опциональные данные о вызовах функций.
    /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/post-chat"/></para>
    /// </remarks>
    public class ChatMessage
    {
        /// <summary>
        /// Получает или задает роль отправителя сообщения.
        /// </summary>
        /// <value>
        /// Роль отправителя: "user" (пользователь), "assistant" (модель), "system" (системное сообщение), "function" (результат вызова функции).
        /// </value>
        [JsonPropertyName("role")]
        public string Role { get; set; }

        /// <summary>
        /// Получает или задает текстовое содержимое сообщения.
        /// </summary>
        /// <value>Текст сообщения.</value>
        [JsonPropertyName("content")]
        public string Content { get; set; }

        /// <summary>
        /// Получает или задает идентификатор, объединяющий массив функций, переданных в запросе.
        /// </summary>
        /// <value>UUID состояния функций или <c>null</c>, если функции не используются.</value>
        [JsonPropertyName("functions_state_id")]
        public Guid? FunctionsStateId { get; set; }

        /// <summary>
        /// Получает или задает информацию о вызове функции моделью.
        /// </summary>
        /// <value>Объект вызова функции или <c>null</c>, если функция не вызывалась.</value>
        [JsonPropertyName("function_call")]
        public FunctionCall? FunctionCall { get; set; }

        /// <summary>
        /// Получает или задает массив идентификаторов файлов для обработки.
        /// </summary>
        /// <value>Список ID файлов (изображений, документов) или <c>null</c>.</value>
        [JsonPropertyName("attachments")]
        public List<string>? Attachments { get; set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ChatMessage"/> (конструктор без параметров для десериализации).
        /// </summary>
        public ChatMessage()
        {
            Role = string.Empty;
            Content = string.Empty;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ChatMessage"/>.
        /// </summary>
        /// <param name="role">Роль отправителя сообщения ("user", "assistant", "system", "function").</param>
        /// <param name="content">Текст сообщения.</param>
        /// <param name="functionsStateId">Идентификатор состояния функций (опционально).</param>
        /// <param name="functionCall">Информация о вызове функции (опционально).</param>
        public ChatMessage(string role, string content, Guid? functionsStateId = null, FunctionCall? functionCall = null)
        {
            this.Role = role;
            this.Content = content;
            this.FunctionsStateId = functionsStateId;
            this.FunctionCall = functionCall;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ChatMessage"/> с использованием enum для роли.
        /// </summary>
        /// <param name="role">Роль отправителя сообщения.</param>
        /// <param name="content">Текст сообщения.</param>
        /// <param name="functionsStateId">Идентификатор состояния функций (опционально).</param>
        /// <param name="functionCall">Информация о вызове функции (опционально).</param>
        public ChatMessage(MessageRole role, string content, Guid? functionsStateId = null, FunctionCall? functionCall = null)
        {
            this.Role = role.ToString().ToLowerInvariant();
            this.Content = content;
            this.FunctionsStateId = functionsStateId;
            this.FunctionCall = functionCall;
        }
    }
}
