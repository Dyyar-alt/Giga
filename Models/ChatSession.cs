using System.Collections.ObjectModel;

namespace Giga.Models
{
    public class ChatSession
    {
        public string Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public ObservableCollection<ChatMessage> Messages { get; set; }

        public ChatSession()
        {
            Id = Guid.NewGuid().ToString();
            StartTime = DateTime.Now;
            Messages = new ObservableCollection<ChatMessage>();
        }

        public string Preview
        {
            get
            {
                var firstMessage = Messages?.FirstOrDefault(m => m.Sender == "User");
                if (firstMessage == null) return "Нет сообщений";
                return firstMessage.Text.Length > 50
                    ? firstMessage.Text.Substring(0, 47) + "..."
                    : firstMessage.Text;
            }
        }
    }
}