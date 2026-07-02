namespace GigaChatSDK.Models.ApiResponse
{
    /// <summary>
    /// Представляет ответ API на запрос подсчета токенов.
    /// </summary>
    /// <remarks>
    /// Содержит массив объектов с информацией о количестве токенов для каждой строки из запроса.
    /// Индекс элемента в массиве соответствует индексу строки в исходном запросе.
    /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/post-tokens-count"/></para>
    /// </remarks>
    public class TokensCountResponse
    {
        /// <summary>
        /// Получает или задает коллекцию результатов подсчета токенов.
        /// </summary>
        /// <value>
        /// Список объектов <see cref="TokenCountItem"/>, каждый из которых содержит информацию
        /// о токенах и символах для соответствующей строки.
        /// </value>
        public IReadOnlyCollection<TokenCountItem> Tokens { get; set; }

        /// <summary>
        /// Создает новый экземпляр ответа на запрос подсчета токенов.
        /// </summary>
        public TokensCountResponse()
        {
            Tokens = new List<TokenCountItem>();
        }
    }
}
