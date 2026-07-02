using System.Text;

namespace Giga.Helpers
{
    public static class TextHelper
    {
        public static string WrapText(string text, int maxWidth)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxWidth)
                return text;

            var result = new StringBuilder();
            var lines = new List<string>();
            var words = text.Split(' ');
            var currentLine = new List<string>();
            var currentLength = 0;

            foreach (var word in words)
            {
                var spaceNeeded = currentLine.Count > 0 ? 1 : 0;

                if (currentLength + word.Length + spaceNeeded <= maxWidth)
                {
                    currentLine.Add(word);
                    currentLength += word.Length + spaceNeeded;
                }
                else
                {
                    if (currentLine.Count > 0)
                    {
                        lines.Add(string.Join(" ", currentLine));
                    }
                    currentLine = new List<string> { word };
                    currentLength = word.Length;
                }
            }

            if (currentLine.Count > 0)
            {
                lines.Add(string.Join(" ", currentLine));
            }

            return string.Join("\n", lines);
        }

        public static string StripHtml(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return System.Text.RegularExpressions.Regex.Replace(text, @"<[^>]+>", "");
        }
    }
}