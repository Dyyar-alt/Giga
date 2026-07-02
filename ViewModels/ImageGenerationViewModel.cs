using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Giga.Services;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Giga.ViewModels
{
    public class GenerationHistoryItem
    {
        public string Prompt { get; set; } = string.Empty;
        public byte[]? ImageBytes { get; set; }
        public DateTime Timestamp { get; set; }
        public string? SavedPath { get; set; }
        public bool IsSaved => !string.IsNullOrEmpty(SavedPath);
    }

    public partial class ImageGenerationViewModel : ObservableObject
    {
        private readonly IApiKeyService _apiKeyService;
        private readonly IImageSaveService _imageSaveService;
        private GigaChatService? _gigachatService;

        private string _prompt = "Вселенная в момент зарождения";
        private bool _isGenerating;
        private string _statusMessage = "Инициализация...";
        private string _selectedStyle = "Аниме";
        private string _selectedResolution = "1024x1024";
        private GenerationHistoryItem? _selectedHistoryItem;
        private bool _hasImage;

        public string Prompt
        {
            get => _prompt;
            set => SetProperty(ref _prompt, value);
        }

        public bool IsGenerating
        {
            get => _isGenerating;
            set
            {
                SetProperty(ref _isGenerating, value);
                OnPropertyChanged(nameof(IsNotGenerating));
            }
        }

        public bool IsNotGenerating => !IsGenerating;

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public string SelectedStyle
        {
            get => _selectedStyle;
            set => SetProperty(ref _selectedStyle, value);
        }

        public string SelectedResolution
        {
            get => _selectedResolution;
            set => SetProperty(ref _selectedResolution, value);
        }

        public GenerationHistoryItem? SelectedHistoryItem
        {
            get => _selectedHistoryItem;
            set
            {
                SetProperty(ref _selectedHistoryItem, value);
                if (value != null)
                {
                    HasImage = value.ImageBytes != null && value.ImageBytes.Length > 0;
                }
                else
                {
                    HasImage = false;
                }
            }
        }

        public bool HasImage
        {
            get => _hasImage;
            set => SetProperty(ref _hasImage, value);
        }

        public ObservableCollection<GenerationHistoryItem> GenerationHistory { get; } = new();
        public ObservableCollection<string> StylesList { get; } = new();
        public ObservableCollection<string> ResolutionsList { get; } = new();

        public IRelayCommand GenerateCommand { get; }
        public IRelayCommand ClearHistoryCommand { get; }
        public IAsyncRelayCommand<GenerationHistoryItem> SaveImageCommand { get; }
        public IAsyncRelayCommand<GenerationHistoryItem> SaveImageWithPickerCommand { get; }

        public ImageGenerationViewModel(IApiKeyService apiKeyService, IImageSaveService imageSaveService)
        {
            _apiKeyService = apiKeyService;
            _imageSaveService = imageSaveService;

            StylesList.Add("Реалистичный");
            StylesList.Add("Аниме");
            StylesList.Add("Киберпанк");
            StylesList.Add("Акварель");
            StylesList.Add("Фэнтези");

            ResolutionsList.Add("1024x1024");
            ResolutionsList.Add("1792x1024");
            ResolutionsList.Add("1024x1792");

            GenerateCommand = new AsyncRelayCommand(GenerateImageAsync);
            ClearHistoryCommand = new RelayCommand(ClearHistory);
            SaveImageCommand = new AsyncRelayCommand<GenerationHistoryItem>(SaveImageAsync);
            SaveImageWithPickerCommand = new AsyncRelayCommand<GenerationHistoryItem>(SaveImageWithPickerAsync);

            _ = InitializeAsync();
        }

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
                    StatusMessage = $"❌ Ошибка: {ex.Message}";
                }
            }
            else
            {
                StatusMessage = "⚠️ Ключ API не найден.";
            }
        }

        private async Task GenerateImageAsync()
        {
            if (IsGenerating || string.IsNullOrWhiteSpace(Prompt) || _gigachatService == null)
                return;

            IsGenerating = true;
            HasImage = false;
            StatusMessage = "⏳ Генерация изображения...";

            try
            {
                var fullPrompt = $"{Prompt}. Стиль: {SelectedStyle}. Размер: {SelectedResolution}";
                System.Diagnostics.Debug.WriteLine($"📝 Запрос: {fullPrompt}");

                var response = await _gigachatService.SendMessageAsync(fullPrompt);
                System.Diagnostics.Debug.WriteLine($"📝 Ответ: {response.Substring(0, Math.Min(200, response.Length))}...");

                var fileId = ExtractFileId(response);
                if (string.IsNullOrEmpty(fileId))
                {
                    StatusMessage = "❌ В ответе нет ID изображения.";
                    HasImage = false;
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"✅ Найден ID файла: {fileId}");

                StatusMessage = "⏳ Скачивание изображения...";
                var imageBytes = await _gigachatService.DownloadImageAsync(fileId);

                if (imageBytes == null || imageBytes.Length == 0)
                {
                    StatusMessage = "❌ Не удалось загрузить изображение.";
                    HasImage = false;
                    return;
                }

                var historyItem = new GenerationHistoryItem
                {
                    Prompt = Prompt,
                    ImageBytes = imageBytes,
                    Timestamp = DateTime.Now
                };

                GenerationHistory.Insert(0, historyItem);
                SelectedHistoryItem = historyItem;
                HasImage = true;
                StatusMessage = $"✅ Изображение сгенерировано! ({imageBytes.Length / 1024} КБ)";
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Ошибка: {ex.Message}";
                HasImage = false;
                System.Diagnostics.Debug.WriteLine($"❌ Исключение: {ex}");
            }
            finally
            {
                IsGenerating = false;
            }
        }

        private string? ExtractFileId(string message)
        {
            if (string.IsNullOrEmpty(message)) return null;

            var guidPattern = @"[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12}";
            var guidMatch = Regex.Match(message, guidPattern, RegexOptions.IgnoreCase);
            if (guidMatch.Success)
            {
                System.Diagnostics.Debug.WriteLine($"✅ Найден GUID: {guidMatch.Value}");
                return guidMatch.Value;
            }

            var hexPattern = @"[a-fA-F0-9]{32,}";
            var hexMatch = Regex.Match(message, hexPattern, RegexOptions.IgnoreCase);
            if (hexMatch.Success)
            {
                System.Diagnostics.Debug.WriteLine($"✅ Найден HEX ID: {hexMatch.Value}");
                return hexMatch.Value;
            }

            System.Diagnostics.Debug.WriteLine($"⚠️ ID не найден в ответе");
            return null;
        }

        private void ClearHistory()
        {
            GenerationHistory.Clear();
            SelectedHistoryItem = null;
            HasImage = false;
            StatusMessage = "История очищена";
        }

        private async Task SaveImageAsync(GenerationHistoryItem? item)
        {
            if (item?.ImageBytes == null)
            {
                await Application.Current!.MainPage!.DisplayAlert("Ошибка", "Нет данных изображения", "ОК");
                return;
            }

            try
            {
                var savedPath = await _imageSaveService.SaveImageAsync(item.ImageBytes, "png");
                if (!string.IsNullOrEmpty(savedPath))
                {
                    item.SavedPath = savedPath;
                    StatusMessage = $"✅ Изображение сохранено!";
                    await Application.Current.MainPage.DisplayAlert(
                        "Успешно!",
                        $"Изображение сохранено в формате PNG",
                        "ОК");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Ошибка",
                        "Не удалось сохранить изображение",
                        "ОК");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Ошибка",
                    $"Ошибка сохранения: {ex.Message}",
                    "ОК");
            }
        }

        private async Task SaveImageWithPickerAsync(GenerationHistoryItem? item)
        {
            if (item?.ImageBytes == null)
            {
                await Application.Current!.MainPage!.DisplayAlert("Ошибка", "Нет данных изображения", "ОК");
                return;
            }

            await _imageSaveService.SaveImageWithPickerAsync(item.ImageBytes, "png");
        }
    }
}