namespace Giga.Services
{
    public class GlobalStatsService
    {
        private int _totalChatMessages = 0;
        private int _totalChatTokens = 0;
        private int _totalImages = 0;
        private int _totalImageTokens = 0;
        private DateTime _appStartTime = DateTime.Now;

        public event EventHandler? StatsChanged;

        public void AddChatMessage(int tokens = 0)
        {
            _totalChatMessages++;
            _totalChatTokens += tokens;
            System.Diagnostics.Debug.WriteLine($"📊 Чат: +1 сообщ., токенов: {tokens}, всего: {_totalChatMessages}");
            StatsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void AddImage(int tokens = 0)
        {
            _totalImages++;
            _totalImageTokens += tokens;
            System.Diagnostics.Debug.WriteLine($"📊 Генерация: +1 карт., токенов: {tokens}, всего: {_totalImages}, всего токенов: {_totalImageTokens}");
            StatsChanged?.Invoke(this, EventArgs.Empty);
        }

        public string GetStats()
        {
            var duration = DateTime.Now - _appStartTime;
            var totalTokens = _totalChatTokens + _totalImageTokens;

            string durationStr;
            if (duration.TotalMinutes < 1)
                durationStr = $"{duration.Seconds}с";
            else if (duration.TotalHours < 1)
                durationStr = $"{duration.Minutes}м {duration.Seconds}с";
            else
                durationStr = $"{duration.Hours}ч {duration.Minutes}м";

            return $"💬 {_totalChatMessages}  🎨 {_totalImages}  🔢 {totalTokens}  ⏱️ {durationStr}";
        }

        public void Reset()
        {
            _totalChatMessages = 0;
            _totalChatTokens = 0;
            _totalImages = 0;
            _totalImageTokens = 0;
            _appStartTime = DateTime.Now;
            StatsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}