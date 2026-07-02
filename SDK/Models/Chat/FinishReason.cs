using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace GigaChatSDK.Models.Chat
{
    /// <summary>
    /// Определяет причину завершения генерации ответа моделью GigaChat.
    /// </summary>
    /// <remarks>
    /// Используется в <see cref="ChatChoice.FinishReason"/> для указания, почему модель завершила генерацию ответа.
    /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/post-chat"/></para>
    /// </remarks>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FinishReason
    {
        /// <summary>
        /// Модель естественным образом завершила генерацию ответа.
        /// </summary>
        /// <remarks>
        /// Это нормальное завершение — модель считает, что полностью ответила на запрос.
        /// </remarks>
        [EnumMember(Value = "stop")]
        Stop,

        /// <summary>
        /// Генерация прервана из-за достижения лимита токенов (<c>max_tokens</c>).
        /// </summary>
        /// <remarks>
        /// Модель могла бы сгенерировать больше текста, но достигнут максимальный лимит токенов в запросе.
        /// Для получения полного ответа увеличьте параметр <c>max_tokens</c> в запросе.
        /// </remarks>
        [EnumMember(Value = "length")]
        Length,

        /// <summary>
        /// Модель вызвала функцию (function calling).
        /// </summary>
        /// <remarks>
        /// Модель решила использовать одну из доступных функций, указанных в запросе.
        /// Поле <see cref="ChatMessage.FunctionCall"/> будет содержать информацию о вызванной функции и её аргументах.
        /// </remarks>
        [EnumMember(Value = "function_call")]
        FunctionCall,

        /// <summary>
        /// Генерация ответа ещё не завершена (при использовании streaming).
        /// </summary>
        /// <remarks>
        /// Используется в режиме потоковой передачи (streaming) для промежуточных сообщений.
        /// Финальное сообщение будет иметь одну из других причин завершения.
        /// </remarks>
        [EnumMember(Value = "null")]
        Null
    }
}
