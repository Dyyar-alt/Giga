using Giga.Helpers; // Используем ваш TextHelper для очистки текста от тегов
using System.Text;

namespace Giga.Services
{
    /// <summary>
    /// Сервис для форматирования диалога в HTML-код.
    /// Хранит текущее состояние чата в виде строки HTML.
    /// </summary>
    public class HtmlMessageFormatter
    {
        private readonly StringBuilder _htmlContent;

        // Базовый CSS-стиль для всего чата
        private const string BaseStyle = @"
            <style>
                body { font-family: 'Segoe UI', Ubuntu, Arial, sans-serif; font-size: 14pt; line-height: 1.6; padding: 10px; margin: 0; background-color: #f8f9fa; }
                .user-message { text-align: right; color: green; }
                .bot-message { text-align: left; color: red; }
                .message-bubble { display: inline-block; max-width: 75%; padding: 8px 12px; border-radius: 18px; margin-bottom: 12px; word-wrap: break-word; }
                .user-message .bubble { background-color: #dcf8c6; }
                .bot-message .bubble { background-color: #ffffff; box-shadow: 0 1px 3px rgba(0,0,0,0.12); }
            </style>";

        public HtmlMessageFormatter()
        {
            _htmlContent = new StringBuilder();
            _htmlContent.AppendLine("<!DOCTYPE html><html><head>" + BaseStyle + "</head><body>");
        }

        /// <summary>
        /// Возвращает текущий HTML-код всего диалога.
        /// Используется для привязки к свойству Source WebView.
        /// </summary>
        public string GetHtml() => _htmlContent.ToString() + "</body></html>";

        /// <summary>
        /// Добавляет сообщение пользователя в диалог.
        /// </summary>
        public void AppendUserMessage(string message)
        {
            var cleanMessage = PrepareText(message);
            var htmlFragment = $@"
                <div class='message user-message'>
                    <span class='bubble'>{cleanMessage}</span>
                </div>";
            _htmlContent.Append(htmlFragment);
        }

        /// <summary>
        /// Добавляет ответ бота в диалог.
        /// </summary>
        public void AppendBotResponse(string response)
        {
            var cleanResponse = PrepareText(response);
            var htmlFragment = $@"
                <div class='message bot-message'>
                    <span class='bubble'>{cleanResponse}</span>
                </div>";
            _htmlContent.Append(htmlFragment);
        }

        /// <summary>
        /// Очищает весь диалог.
        /// </summary>
        public void Clear()
        {
            _htmlContent.Clear();
            _htmlContent.AppendLine("<!DOCTYPE html><html><head>" + BaseStyle + "</head><body>");
        }

        /// <summary>
        /// Подготавливает текст для безопасной вставки в HTML:
        /// - Удаляет существующие HTML-теги
        /// - Экранирует спецсимволы (&, <, >, ")
        /// - Заменяет переносы строк на <br/>
        /// </summary>
        private static string PrepareText(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            // Используем ваш TextHelper для удаления тегов
            var noTags = TextHelper.StripHtml(text);
            // Экранируем спецсимволы
            var escaped = System.Net.WebUtility.HtmlEncode(noTags);
            // Заменяем \n на <br/> для сохранения переносов строк
            return escaped.Replace("\n", "<br/>");
        }
    }
}