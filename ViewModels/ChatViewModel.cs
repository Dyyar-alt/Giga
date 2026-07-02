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
        private GigaChatService? _gigachatService;

        [ObservableProperty]
        private string userPrompt = string.Empty;

        [ObservableProperty]
        private bool isProcessing;

        [ObservableProperty]
        private string statusMessage = "Инициализация...";

        [ObservableProperty]
        private ObservableCollection<ChatSession> savedSessions = new();

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

        public ChatViewModel(IApiKeyService apiKeyService, ChatStorageService storageService)
        {
            _apiKeyService = apiKeyService;
            _storageService = storageService;

            var sessions = _storageService.GetAllSessions();
            SavedSessions = new ObservableCollection<ChatSession>(sessions);
            CurrentSession = new ChatSession();

            SendCommand = new AsyncRelayCommand(SendMessageAsync, () => !IsProcessing && !string.IsNullOrWhiteSpace(UserPrompt));
            SaveSessionCommand = new RelayCommand(SaveCurrentSession, CanSaveSession);
            LoadSessionCommand = new RelayCommand<ChatSession>(LoadSelectedSession);
            ClearChatCommand = new RelayCommand(ClearCurrentChat);

            _ = InitializeAsync();
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
            var apiKey = await _apiKeyService.GetApiKeyAsync();
            if (!string.IsNullOrEmpty(apiKey))
            {
                try
                {
                    _gigachatService = new GigaChatService(apiKey);
                    await _gigachatService.InitializeAsync();
                    StatusMessage = "✅ Готов к работе!";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"❌ Ошибка инициализации: {ex.Message}";
                }
            }
            else
            {
                StatusMessage = "⚠️ Ключ API не найден.";
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
                StatusMessage = "✅ Ответ получен";
            }
            catch (Exception ex)
            {
                AddMessage("Bot", $"❌ Ошибка: {ex.Message}");
                StatusMessage = $"❌ Ошибка: {ex.Message}";
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
        }

        private void SaveCurrentSession()
        {
            if (CurrentSession == null || CurrentSession.Messages.Count == 0) return;

            CurrentSession.EndTime = DateTime.Now;
            _ = _storageService.SaveChatAsync(CurrentSession);
            SavedSessions.Insert(0, CurrentSession);
            CurrentSession = new ChatSession();
            SaveSessionCommand.NotifyCanExecuteChanged();
        }

        private void LoadSelectedSession(ChatSession? session)
        {
            if (session == null) return;
            CurrentSession = session;
            SelectedSession = null;
            OnPropertyChanged(nameof(CurrentSessionMessages));
            StatusMessage = $"✅ Загружена сессия от {session.StartTime:g}";
        }

        private void ClearCurrentChat()
        {
            CurrentSession?.Messages.Clear();
            OnPropertyChanged(nameof(CurrentSessionMessages));
            SaveSessionCommand.NotifyCanExecuteChanged();
            StatusMessage = "🧹 Чат очищен";
        }
    }
}