using System.Text.Json.Serialization;

namespace GigaChatSDK.Models.ApiResponse
{
    /// <summary>
    /// Представляет ответ API на запрос остатка токенов.
    /// </summary>
    /// <remarks>
    /// Содержит массив объектов с информацией об остатке токенов для каждой доступной модели.
    /// Метод доступен только при покупке пакетов токенов.
    /// <para>Документация: <see href="https://developers.sber.ru/docs/ru/gigachat/api/reference/rest/get-balance"/></para>
    /// </remarks>
    public class BalanceResponse
    {
        /// <summary>
        /// Получает или задает коллекцию остатков токенов по моделям.
        /// </summary>
        /// <value>
        /// Список объектов <see cref="BalanceItem"/>, каждый из которых содержит
        /// название модели и остаток токенов.
        /// </value>
        [JsonPropertyName("balance")]
        public IReadOnlyCollection<BalanceItem> Balance { get; set; }

        /// <summary>
        /// Создает новый экземпляр ответа на запрос баланса.
        /// </summary>
        public BalanceResponse()
        {
            Balance = new List<BalanceItem>();
        }
    }
}
