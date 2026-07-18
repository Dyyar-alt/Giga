namespace Giga.Models
{
    public class ChatMessage
    {
        public string Sender { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;

        // Статистика токенов. Каждое сообщение потребляет токены. На сессию выдается 5000 токенов
        public int Tokens { get; set; }
        public int Characters { get; set; }
    }
}
