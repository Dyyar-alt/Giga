using Giga.Services;

namespace Giga.Views;

public partial class StatusBar : ContentView
{
    private readonly GlobalStatsService? _statsService;
    private System.Timers.Timer? _updateTimer;

    public StatusBar()
    {
        InitializeComponent();

        try
        {
            _statsService = Application.Current?.Handler?.MauiContext?.Services
                ?.GetService<GlobalStatsService>();

            if (_statsService != null)
            {
                _statsService.StatsChanged += OnStatsChanged;
                UpdateStats();

                _updateTimer = new System.Timers.Timer(1000);
                _updateTimer.Elapsed += (s, e) =>
                {
                    MainThread.BeginInvokeOnMainThread(UpdateStats);
                };
                _updateTimer.Start();

                System.Diagnostics.Debug.WriteLine("📊 StatusBar инициализирован");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("❌ StatusBar: GlobalStatsService не найден");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка инициализации StatusBar: {ex.Message}");
        }
    }

    private void OnStatsChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(UpdateStats);
    }

    private void UpdateStats()
    {
        try
        {
            if (_statsService != null && StatsLabel != null)
            {
                StatsLabel.Text = _statsService.GetStats();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка обновления статистики: {ex.Message}");
        }
    }

    // Освобождаем ресурсы
    ~StatusBar()
    {
        _updateTimer?.Stop();
        _updateTimer?.Dispose();
        _updateTimer = null;

        if (_statsService != null)
        {
            _statsService.StatsChanged -= OnStatsChanged;
        }
    }
}