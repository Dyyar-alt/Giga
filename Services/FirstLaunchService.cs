namespace Giga.Services
{
    public class FirstLaunchService
    {
        private const string FirstLaunchKey = "is_first_launch";

        public bool IsFirstLaunch()
        {
            try
            {
                // Если ключа нет — значит первый запуск
                return !Preferences.ContainsKey(FirstLaunchKey);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка проверки первого запуска: {ex.Message}");
                return true; // В случае ошибки считаем, что первый запуск
            }
        }

        public void MarkLaunchCompleted()
        {
            try
            {
                Preferences.Set(FirstLaunchKey, DateTime.Now.ToString("o"));
                System.Diagnostics.Debug.WriteLine("Первый запуск отмечен");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения статуса запуска: {ex.Message}");
            }
        }
    }
}