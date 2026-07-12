namespace Giga.Models
{
    public class ChatMessage
    {
        public string Sender { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;

        // Статистика токенов для сообщения
        public int Tokens { get; set; }
        public int Characters { get; set; }
    }
}