using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Giga.Models;
using Giga.Services;
using System.Collections.ObjectModel;

namespace Giga.ViewModels
{
    public partial class ChatViewModel : ObservableObject
    {
        private readonly IApiKeyService _apiKeyService;
        private readonly ChatStorageService _storageService;
        private readonly GlobalStatsService _statsService;
        private GigaChatService? _gigachatService;
        private Timer? _autoSaveTimer;
        private const int AutoSaveInterval = 5000;

        [ObservableProperty]
        private string userPrompt = string.Empty;

        [ObservableProperty]
        private bool isProcessing;

        [ObservableProperty]
        private string statusMessage = "Инициализация...";

        [ObservableProperty]
        private ObservableCollection<ChatSession> savedSessions = new();

        [ObservableProperty]
        private bool isHistoryVisible = false;

        private ChatSession? _currentSession;

        [ObservableProperty]
        private ChatSession? selectedSession;

        public ChatSession? CurrentSession
        {
            get => _currentSession;
            set
            {
                if (_currentSession != value)
                {
                    _currentSession = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CurrentSessionMessages));
                }
            }
        }

        public ObservableCollection<ChatMessage>? CurrentSessionMessages => CurrentSession?.Messages;

        public IAsyncRelayCommand SendCommand { get; }
        public IRelayCommand SaveSessionCommand { get; }
        public IRelayCommand<ChatSession> LoadSessionCommand { get; }
        public IRelayCommand ClearChatCommand { get; }
        public IRelayCommand NewChatCommand { get; }
        public IRelayCommand ToggleHistoryCommand { get; }
        public IRelayCommand ClearAllHistoryCommand { get; }
        public IRelayCommand RefreshHistoryCommand { get; }
        public IRelayCommand ShowTokenStatsCommand { get; }

        public ChatViewModel(IApiKeyService apiKeyService, ChatStorageService storageService, GlobalStatsService statsService)
        {
            _apiKeyService = apiKeyService;
            _storageService = storageService;
            _statsService = statsService;

            LoadSavedSessions();
            InitializeCurrentSession();

            SendCommand = new AsyncRelayCommand(SendMessageAsync, () => !IsProcessing && !string.IsNullOrWhiteSpace(UserPrompt));
            SaveSessionCommand = new RelayCommand(SaveCurrentSession, CanSaveSession);
            LoadSessionCommand = new RelayCommand<ChatSession>(LoadSelectedSession);
            ClearChatCommand = new RelayCommand(ClearCurrentChat);
            NewChatCommand = new RelayCommand(CreateNewChat);
            ToggleHistoryCommand = new RelayCommand(ToggleHistory);
            ClearAllHistoryCommand = new RelayCommand(ClearAllHistory);
            RefreshHistoryCommand = new RelayCommand(RefreshHistory);
            ShowTokenStatsCommand = new RelayCommand(ShowTokenStats);

            StartAutoSave();
            _ = InitializeAsync();
        }

        private async void ResetStatusAfterDelay(string defaultMessage = "✅ Готов к работе!")
        {
            try
            {
                await Task.Delay(2000);
                StatusMessage = defaultMessage;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сброса статуса: {ex.Message}");
            }
        }

        private async void ShowTokenStats()
        {
            try
            {
                if (CurrentSession == null || CurrentSession.Messages.Count == 0)
                {
                    await Application.Current!.MainPage!.DisplayAlert(
                        "📊 Статистика сессии",
                        "Нет сообщений в текущей сессии",
                        "ОК");
                    return;
                }

                var totalChars = CurrentSession.Messages.Sum(m => m.Text.Length);
                var messageCount = CurrentSession.Messages.Count;
                var userMessages = CurrentSession.Messages.Count(m => m.Sender == "User");
                var botMessages = CurrentSession.Messages.Count(m => m.Sender == "Bot");

                var userTokens = 0;
                var botTokens = 0;
                foreach (var msg in CurrentSession.Messages)
                {
                    if (msg.Tokens > 0)
                    {
                        if (msg.Sender == "User")
                            userTokens += msg.Tokens;
                        else if (msg.Sender == "Bot")
                            botTokens += msg.Tokens;
                    }
                }

                if (userTokens == 0 && botTokens == 0 && _gigachatService != null && _gigachatService.IsInitialized)
                {
                    try
                    {
                        var lastUserMsg = CurrentSession.Messages.LastOrDefault(m => m.Sender == "User");
                        var lastBotMsg = CurrentSession.Messages.LastOrDefault(m => m.Sender == "Bot");
                        if (lastUserMsg != null && lastBotMsg != null)
                        {
                            var tokenInfoResult = await _gigachatService.CountTokensAsync(lastUserMsg.Text, lastBotMsg.Text);
                            if (tokenInfoResult != null)
                            {
                                userTokens = tokenInfoResult.UserTokens;
                                botTokens = tokenInfoResult.BotTokens;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка подсчёта токенов: {ex.Message}");
                    }
                }

                var totalTokens = userTokens + botTokens;

                string statsTokenInfo;
                if (totalTokens > 0)
                {
                    statsTokenInfo = $"""
                        👤 Токенов пользователя: {userTokens}
                        🤖 Токенов бота: {botTokens}
                        🔢 Всего токенов: {totalTokens}
                        """;
                }
                else
                {
                    statsTokenInfo = "📭 Токены не подсчитаны";
                }

                await Application.Current!.MainPage!.DisplayAlert(
                    "📊 Статистика текущей сессии",
                    $"""
                    ─── ДИАЛОГ ───
                    Сообщений: {messageCount}
                    👤 Пользователь: {userMessages}
                    🤖 Бот: {botMessages}
                    📝 Символов: {totalChars}

                    ─── ТОКЕНЫ ───
                    {statsTokenInfo}
                    """,
                    "ОК");
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Ошибка",
                    $"Не удалось получить статистику: {ex.Message}",
                    "ОК");
            }
        }

        private void ToggleHistory()
        {
            IsHistoryVisible = !IsHistoryVisible;
            System.Diagnostics.Debug.WriteLine($"История: {(IsHistoryVisible ? "открыта" : "закрыта")}");

            if (IsHistoryVisible)
            {
                RefreshHistory();
            }
        }

        private void RefreshHistory()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Обновление истории...");
                LoadSavedSessions();
                StatusMessage = $"📋 История обновлена ({SavedSessions.Count} сессий)";
                ResetStatusAfterDelay();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления истории: {ex.Message}");
                StatusMessage = $"❌ Ошибка обновления: {ex.Message}";
                ResetStatusAfterDelay();
            }
        }

        private async void ClearAllHistory()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Очистка истории. Сессий: {SavedSessions.Count}");

                if (SavedSessions.Count == 0)
                {
                    StatusMessage = "📭 История уже пуста";
                    await Application.Current!.MainPage!.DisplayAlert("Информация", "История уже пуста", "ОК");
                    ResetStatusAfterDelay();
                    return;
                }

                bool answer = await Application.Current!.MainPage!.DisplayAlert(
                    "Очистка истории",
                    $"Вы уверены, что хотите удалить все {SavedSessions.Count} сохранённых сессий?\nЭто действие нельзя отменить!",
                    "Да, удалить всё",
                    "Отмена"
                );

                if (!answer) return;

                var historyPath = Path.Combine(FileSystem.AppDataDirectory, "ChatHistory");
                System.Diagnostics.Debug.WriteLine($"Путь к истории: {historyPath}");

                int deletedCount = 0;

                if (Directory.Exists(historyPath))
                {
                    var files = Directory.GetFiles(historyPath, "*.json");
                    System.Diagnostics.Debug.WriteLine($"Найдено файлов: {files.Length}");

                    foreach (var file in files)
                    {
                        try
                        {
                            File.Delete(file);
                            deletedCount++;
                            System.Diagnostics.Debug.WriteLine($"Удалён файл: {Path.GetFileName(file)}");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Ошибка удаления файла {file}: {ex.Message}");
                        }
                    }
                }

                SavedSessions.Clear();
                OnPropertyChanged(nameof(SavedSessions));

                StatusMessage = $"🧹 Удалено {deletedCount} сессий";
                ResetStatusAfterDelay();

                CurrentSession = new ChatSession();
                OnPropertyChanged(nameof(CurrentSessionMessages));
                SaveSessionCommand.NotifyCanExecuteChanged();

                RefreshHistory();

                if (IsHistoryVisible)
                {
                    ToggleHistory();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка удаления истории: {ex.Message}");
                StatusMessage = $"❌ Ошибка удаления: {ex.Message}";
                ResetStatusAfterDelay();
            }
        }

        private void LoadSavedSessions()
        {
            try
            {
                var sessions = _storageService.GetAllSessions();

                SavedSessions.Clear();
                foreach (var session in sessions)
                {
                    SavedSessions.Add(session);
                }

                OnPropertyChanged(nameof(SavedSessions));

                System.Diagnostics.Debug.WriteLine($"Загружено {SavedSessions.Count} сохранённых сессий");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки сессий: {ex.Message}");
                StatusMessage = $"❌ Ошибка загрузки: {ex.Message}";
                ResetStatusAfterDelay();
            }
        }

        private void InitializeCurrentSession()
        {
            try
            {
                if (SavedSessions.Any())
                {
                    var lastSession = SavedSessions.FirstOrDefault();
                    if (lastSession != null && lastSession.Messages.Any())
                    {
                        CurrentSession = lastSession;
                        StatusMessage = $"✅ Загружена последняя сессия ({lastSession.Messages.Count} сообщений)";
                        System.Diagnostics.Debug.WriteLine($"Загружена последняя сессия: {lastSession.Id}, сообщений: {lastSession.Messages.Count}");
                        return;
                    }
                }

                CurrentSession = new ChatSession();
                StatusMessage = "💬 Новая сессия";
                System.Diagnostics.Debug.WriteLine("Создана новая сессия");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации сессии: {ex.Message}");
                CurrentSession = new ChatSession();
                StatusMessage = "💬 Новая сессия";
            }
        }

        private void StartAutoSave()
        {
            try
            {
                _autoSaveTimer = new Timer(_ =>
                {
                    if (CurrentSession?.Messages.Count > 0)
                    {
                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            await AutoSaveCurrentSession();
                        });
                    }
                }, null, AutoSaveInterval, AutoSaveInterval);
                System.Diagnostics.Debug.WriteLine("Автосохранение запущено");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка запуска автосохранения: {ex.Message}");
            }
        }

        private async Task AutoSaveCurrentSession()
        {
            try
            {
                if (CurrentSession != null && CurrentSession.Messages.Count > 0)
                {
                    await _storageService.SaveChatAsync(CurrentSession);
                    System.Diagnostics.Debug.WriteLine($"Автосохранение: {CurrentSession.Messages.Count} сообщений");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка автосохранения: {ex.Message}");
            }
        }

        private void CreateNewChat()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Создание нового чата");

                if (CurrentSession?.Messages.Count > 0)
                {
                    SaveCurrentSession();
                }

                CurrentSession = new ChatSession();
                StatusMessage = "💬 Новая сессия";
                OnPropertyChanged(nameof(CurrentSessionMessages));
                SaveSessionCommand.NotifyCanExecuteChanged();

                if (IsHistoryVisible)
                {
                    ToggleHistory();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка создания нового чата: {ex.Message}");
                StatusMessage = $"❌ Ошибка: {ex.Message}";
                ResetStatusAfterDelay();
            }
        }

        partial void OnUserPromptChanged(string value) => SendCommand.NotifyCanExecuteChanged();

        partial void OnIsProcessingChanged(bool value)
        {
            SendCommand.NotifyCanExecuteChanged();
            SaveSessionCommand.NotifyCanExecuteChanged();
        }

        private bool CanSaveSession() => CurrentSession?.Messages?.Count > 0;

        private async Task InitializeAsync()
        {
            try
            {
                var apiKey = await _apiKeyService.GetApiKeyAsync();
                if (!string.IsNullOrEmpty(apiKey))
                {
                    try
                    {
                        _gigachatService = new GigaChatService(apiKey);
                        await _gigachatService.InitializeAsync();
                        StatusMessage = "✅ Готов к работе!";
                        System.Diagnostics.Debug.WriteLine("GigaChatService инициализирован");
                    }
                    catch (Exception ex)
                    {
                        StatusMessage = $"❌ Ошибка инициализации: {ex.Message}";
                        System.Diagnostics.Debug.WriteLine($"Ошибка инициализации GigaChatService: {ex.Message}");
                    }
                }
                else
                {
                    StatusMessage = "⚠️ Ключ API не найден.";
                    System.Diagnostics.Debug.WriteLine("API ключ не найден");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в InitializeAsync: {ex.Message}");
                StatusMessage = $"❌ Ошибка: {ex.Message}";
            }
        }

        private async Task SendMessageAsync()
        {
            if (IsProcessing || string.IsNullOrWhiteSpace(UserPrompt) || _gigachatService == null)
                return;

            IsProcessing = true;
            var userMessage = UserPrompt;
            UserPrompt = string.Empty;

            try
            {
                AddMessage("User", userMessage);
                var botResponse = await _gigachatService.SendMessageAsync(userMessage);
                AddMessage("Bot", botResponse);

                int tokens = 0;
                try
                {
                    var tokenInfoResult = await _gigachatService.CountTokensAsync(userMessage, botResponse);
                    if (tokenInfoResult != null && CurrentSession != null)
                    {
                        CurrentSession.TotalTokens = tokenInfoResult.TotalTokens;
                        CurrentSession.TotalCharacters = tokenInfoResult.TotalCharacters;
                        tokens = tokenInfoResult.TotalTokens;
                        StatusMessage = $"✅ Ответ получен (🔢 {tokenInfoResult.TotalTokens} токенов)";
                    }
                    else
                    {
                        StatusMessage = "✅ Ответ получен";
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка подсчёта токенов: {ex.Message}");
                    StatusMessage = "✅ Ответ получен (токены не подсчитаны)";
                }

                _statsService.AddChatMessage(tokens);

                await AutoSaveCurrentSession();
            }
            catch (Exception ex)
            {
                AddMessage("Bot", $"❌ Ошибка: {ex.Message}");
                StatusMessage = $"❌ Ошибка: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Ошибка отправки сообщения: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
                SaveSessionCommand.NotifyCanExecuteChanged();
            }
        }

        private void AddMessage(string sender, string text)
        {
            if (CurrentSession == null) return;

            CurrentSession.Messages.Add(new ChatMessage
            {
                Sender = sender,
                Text = text,
                Timestamp = DateTime.Now
            });

            OnPropertyChanged(nameof(CurrentSessionMessages));
            OnPropertyChanged(nameof(CurrentSession));

            System.Diagnostics.Debug.WriteLine($"Добавлено сообщение от {sender}, всего: {CurrentSession.Messages.Count}");
        }

        private void SaveCurrentSession()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Сохранение сессии. Сообщений: {CurrentSession?.Messages.Count ?? 0}");

                if (CurrentSession == null || CurrentSession.Messages.Count == 0)
                {
                    StatusMessage = "⚠️ Нет сообщений для сохранения";
                    ResetStatusAfterDelay();
                    return;
                }

                CurrentSession.EndTime = DateTime.Now;

                var existing = SavedSessions.FirstOrDefault(s => s.Id == CurrentSession.Id);
                if (existing != null)
                {
                    var index = SavedSessions.IndexOf(existing);
                    SavedSessions[index] = CurrentSession;
                    System.Diagnostics.Debug.WriteLine($"Обновлена существующая сессия: {CurrentSession.Id}");
                }
                else
                {
                    SavedSessions.Insert(0, CurrentSession);
                    System.Diagnostics.Debug.WriteLine($"Добавлена новая сессия: {CurrentSession.Id}");
                }

                _ = _storageService.SaveChatAsync(CurrentSession);
                StatusMessage = "💾 Сессия сохранена";
                ResetStatusAfterDelay();

                CurrentSession = new ChatSession();
                OnPropertyChanged(nameof(CurrentSessionMessages));
                SaveSessionCommand.NotifyCanExecuteChanged();
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Ошибка сохранения: {ex.Message}";
                ResetStatusAfterDelay();
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void LoadSelectedSession(ChatSession? session)
        {
            try
            {
                if (session == null)
                {
                    System.Diagnostics.Debug.WriteLine("Попытка загрузки null сессии");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Загрузка сессии: {session.Id}, сообщений: {session.Messages.Count}");

                if (CurrentSession?.Messages.Count > 0 && CurrentSession.Id != session.Id)
                {
                    SaveCurrentSession();
                }

                CurrentSession = session;
                SelectedSession = null;
                OnPropertyChanged(nameof(CurrentSessionMessages));
                StatusMessage = $"✅ Загружена сессия от {session.StartTime:g} ({session.Messages.Count} сообщений)";
                SaveSessionCommand.NotifyCanExecuteChanged();

                System.Diagnostics.Debug.WriteLine($"Сессия загружена, сообщений: {CurrentSession?.Messages.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки сессии: {ex.Message}");
                StatusMessage = $"❌ Ошибка загрузки: {ex.Message}";
                ResetStatusAfterDelay();
            }
        }

        private void ClearCurrentChat()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Очистка текущего чата");
                CurrentSession?.Messages.Clear();
                OnPropertyChanged(nameof(CurrentSessionMessages));
                SaveSessionCommand.NotifyCanExecuteChanged();
                StatusMessage = "🧹 Чат очищен";
                ResetStatusAfterDelay();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка очистки чата: {ex.Message}");
                StatusMessage = $"❌ Ошибка: {ex.Message}";
                ResetStatusAfterDelay();
            }
        }

        public void Cleanup()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Cleanup вызван");
                _autoSaveTimer?.Dispose();
                _autoSaveTimer = null;

                if (CurrentSession?.Messages.Count > 0)
                {
                    CurrentSession.EndTime = DateTime.Now;
                    _storageService.SaveChatAsync(CurrentSession).Wait();
                    System.Diagnostics.Debug.WriteLine("Финальное сохранение выполнено");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в Cleanup: {ex.Message}");
            }
        }
    }
}