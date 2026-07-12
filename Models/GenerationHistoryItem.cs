namespace Giga.Models
{
    public class GenerationHistoryItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Prompt { get; set; } = string.Empty;
        public string Style { get; set; } = string.Empty;
        public string Resolution { get; set; } = string.Empty;
        public byte[]? ImageBytes { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string? SavedPath { get; set; }
        public bool IsSaved => !string.IsNullOrEmpty(SavedPath);
        public bool HasImage => ImageBytes != null && ImageBytes.Length > 0;
    }
}