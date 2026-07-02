using System.Text.Json.Serialization;

namespace GigaChatSDK.Models.Chat
{
    /// <summary>
    /// Представляет один вариант ответа модели GigaChat.
    /// </summary>
    /// <remarks>
    /// Содержит сгенерированное сообщение, его индекс в массиве ответов и причину завершения генерации.
    /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/post-chat"/></para>
    /// </remarks>
    public class ChatChoice
    {
        /// <summary>
        /// Получает или задает сгенерированное моделью сообщение.
        /// </summary>
        /// <value>Объект <see cref="ChatMessage"/> с ответом модели.</value>
        [JsonPropertyName("message")]
        public ChatMessage? Message { get; set; }

        /// <summary>
        /// Получает или задает индекс данного варианта ответа в массиве <see cref="ChatCompletionResponse.Choices"/>.
        /// </summary>
        /// <value>Индекс варианта ответа (начиная с 0).</value>
        [JsonPropertyName("index")]
        public int Index { get; set; }

        /// <summary>
        /// Получает или задает причину завершения генерации ответа.
        /// </summary>
        /// <value>
        /// Причина завершения:
        /// <list type="bullet">
        /// <item><see cref="Chat.FinishReason.Stop"/> - естественное завершение</item>
        /// <item><see cref="Chat.FinishReason.Length"/> - достигнут лимит токенов</item>
        /// <item><see cref="Chat.FinishReason.FunctionCall"/> - модель вызвала функцию</item>
        /// <item><c>null</c> - генерация не завершена (при использовании streaming)</item>
        /// </list>
        /// </value>
        [JsonPropertyName("finish_reason")]
        public FinishReason? FinishReason { get; set; }
    }
}
