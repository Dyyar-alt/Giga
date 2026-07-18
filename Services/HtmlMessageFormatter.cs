using Giga.Helpers;
using System.Text;

namespace Giga.Services
{

    

    public class HtmlMessageFormatter
    {
        private readonly StringBuilder _htmlContent;

        
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


        

        public string GetHtml() => _htmlContent.ToString() + "</body></html>";


        
        public void AppendUserMessage(string message)
        {
            var cleanMessage = PrepareText(message);
            var htmlFragment = $@"
                <div class='message user-message'>
                    <span class='bubble'>{cleanMessage}</span>
                </div>";
            _htmlContent.Append(htmlFragment);
        }


        
        public void AppendBotResponse(string response)
        {
            var cleanResponse = PrepareText(response);
            var htmlFragment = $@"
                <div class='message bot-message'>
                    <span class='bubble'>{cleanResponse}</span>
                </div>";
            _htmlContent.Append(htmlFragment);
        }


      
        public void Clear()
        {
            _htmlContent.Clear();
            _htmlContent.AppendLine("<!DOCTYPE html><html><head>" + BaseStyle + "</head><body>");
        }


       private static string PrepareText(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
           
            var noTags = TextHelper.StripHtml(text);
           
            var escaped = System.Net.WebUtility.HtmlEncode(noTags);
            
            return escaped.Replace("\n", "<br/>");
        }
    }
}
