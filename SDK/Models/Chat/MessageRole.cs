namespace GigaChatSDK.Models.Chat
{
    /// <summary>
    /// Определяет роли участников диалога с GigaChat.
    /// </summary>
    /// <remarks>
    /// Используется для указания роли отправителя сообщения в чате.
    /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/post-chat"/></para>
    /// </remarks>
    public enum MessageRole
    {
        /// <summary>
        /// Роль пользователя. Используется для сообщений от конечного пользователя.
        /// </summary>
        User,

        /// <summary>
        /// Роль ассистента (модели GigaChat). Используется для ответов модели.
        /// </summary>
        Assistant,

        /// <summary>
        /// Системная роль. Используется для системных инструкций, определяющих поведение модели.
        /// </summary>
        System,

        /// <summary>
        /// Роль функции. Используется для ответов от вызванных функций в режиме Function Calling.
        /// </summary>
        Function
    }
}
