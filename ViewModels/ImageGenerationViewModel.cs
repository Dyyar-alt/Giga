using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Giga.Models;
using Giga.Services;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Giga.ViewModels
{
    public partial class ImageGenerationViewModel : ObservableObject
    {
        private readonly IApiKeyService _apiKeyService;
        private readonly IImageSaveService _imageSaveService;
        private readonly GlobalStatsService _statsService;
        private GigaChatService? _gigachatService;

        [ObservableProperty]
        private string prompt = "Вселенная в момент зарождения";

        [ObservableProperty]
        private bool isGenerating;

        [ObservableProperty]
        private string statusMessage = "Инициализация...";

        [ObservableProperty]
        private string selectedStyle = "Аниме";

        [ObservableProperty]
        private string selectedResolution = "1024x1024";

        [ObservableProperty]
        private GenerationHistoryItem? selectedHistoryItem;

        [ObservableProperty]
        private bool hasImage;

        [ObservableProperty]
        private bool isHistoryVisible = false;

        public bool IsNotGenerating => !IsGenerating;

        public ObservableCollection<GenerationHistoryItem> GenerationHistory { get; } = new();
        public ObservableCollection<string> StylesList { get; } = new();
        public ObservableCollection<string> ResolutionsList { get; } = new();

        public IRelayCommand GenerateCommand { get; }
        public IRelayCommand ClearHistoryCommand { get; }
        public IAsyncRelayCommand<GenerationHistoryItem> SaveImageCommand { get; }
        public IAsyncRelayCommand<GenerationHistoryItem> SaveImageWithPickerCommand { get; }
        public IRelayCommand SaveCurrentImageCommand { get; }
        public IRelayCommand ToggleHistoryCommand { get; }
        public IRelayCommand RefreshHistoryCommand { get; }
        public IRelayCommand ClearAllHistoryCommand { get; }
        public IRelayCommand<GenerationHistoryItem> LoadHistoryItemCommand { get; }
        public IRelayCommand ShowTokenStatsCommand { get; }

        public ImageGenerationViewModel(IApiKeyService apiKeyService, IImageSaveService imageSaveService, GlobalStatsService statsService)
        {
            _apiKeyService = apiKeyService;
            _imageSaveService = imageSaveService;
            _statsService = statsService;

            //Параметры генерации берем из API
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
            SaveCurrentImageCommand = new RelayCommand(SaveCurrentImage);
            ToggleHistoryCommand = new RelayCommand(ToggleHistory);
            RefreshHistoryCommand = new RelayCommand(RefreshHistory);
            ClearAllHistoryCommand = new RelayCommand(ClearAllHistory);
            LoadHistoryItemCommand = new RelayCommand<GenerationHistoryItem>(LoadHistoryItem);
            ShowTokenStatsCommand = new RelayCommand(ShowTokenStats);

            LoadHistoryFromStorage();
            _ = InitializeAsync();
        }

        private async void SaveCurrentImage()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Prompt))
                {
                    await Application.Current!.MainPage!.DisplayAlert(
                        "Ошибка",
                        "Введите описание изображения",
                        "ОК");
                    return;
                }

                // Проверяем, не сохранён ли уже такой же промпт (опционально)
                var existing = GenerationHistory.FirstOrDefault(i => i.Prompt == Prompt && i.Style == SelectedStyle);
                if (existing != null)
                {
                    var answer = await Application.Current!.MainPage!.DisplayAlert(
                        "Повтор",
                        "Такой промпт уже есть в истории. Сохранить ещё раз?",
                        "Да",
                        "Нет");
                    if (!answer) return;
                }

                var historyItem = new GenerationHistoryItem
                {
                    Prompt = Prompt,
                    Style = SelectedStyle,
                    Resolution = SelectedResolution,
                    ImageBytes = SelectedHistoryItem?.ImageBytes,
                    Timestamp = DateTime.Now,
                    SavedPath = SelectedHistoryItem?.SavedPath
                };

                SaveHistoryToStorage(historyItem);
                GenerationHistory.Insert(0, historyItem);

                if (historyItem.HasImage)
                {
                    StatusMessage = "✅ Промпт и изображение сохранены в историю!";
                    await Application.Current!.MainPage!.DisplayAlert(
                        "Успешно!",
                        "Промпт и изображение сохранены в историю",
                        "ОК");
                }
                else
                {
                    StatusMessage = "📝 Промпт сохранён в историю (без изображения)";
                    await Application.Current!.MainPage!.DisplayAlert(
                        "Успешно!",
                        "Промпт сохранён в историю",
                        "ОК");
                }
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Ошибка",
                    $"Не удалось сохранить в историю: {ex.Message}",
                    "ОК");
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения в историю: {ex.Message}");
            }
        }

        private async void ShowTokenStats()
        {
            try
            {
                var sessionItems = GenerationHistory
                    .Where(item => item.Timestamp >= DateTime.Now.AddMinutes(-30))
                    .ToList();

                if (!sessionItems.Any())
                {
                    await Application.Current!.MainPage!.DisplayAlert(
                        "📊 Статистика генерации",
                        "Нет генераций в текущей сессии",
                        "ОК");
                    return;
                }

                var totalImages = sessionItems.Count;
                var imagesWithData = sessionItems.Count(i => i.HasImage);
                var totalSize = sessionItems
                    .Where(i => i.ImageBytes != null)
                    .Sum(i => i.ImageBytes?.Length ?? 0) / 1024;

                string statsTokenInfo = "";
                if (_gigachatService != null && _gigachatService.IsInitialized && sessionItems.Any())
                {
                    try
                    {
                        int totalTokens = 0;
                        var lastItem = sessionItems.FirstOrDefault();
                        if (lastItem != null)
                        {
                            var fullPrompt = $"{lastItem.Prompt}. Стиль: {lastItem.Style}. Размер: {lastItem.Resolution}";
                            var tokenInfoResult = await _gigachatService.CountTokensAsync(fullPrompt, "Изображение сгенерировано");
                            if (tokenInfoResult != null)
                            {
                                totalTokens = tokenInfoResult.TotalTokens * sessionItems.Count;
                            }
                        }
                        statsTokenInfo = $"\n🔢 Примерно токенов: {totalTokens}";
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка подсчёта токенов: {ex.Message}");
                    }
                }

                await Application.Current!.MainPage!.DisplayAlert(
                    "📊 Статистика генерации",
                    $"""
                    ─── ИЗОБРАЖЕНИЯ ───
                    🖼️ Сгенерировано: {totalImages}
                    ✅ Успешных: {imagesWithData}
                    ❌ Неудачных: {totalImages - imagesWithData}
                    💾 Вес: {totalSize} КБ
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
            System.Diagnostics.Debug.WriteLine($"История генерации: {(IsHistoryVisible ? "открыта" : "закрыта")}");

            if (IsHistoryVisible)
            {
                RefreshHistory();
            }
        }

        private void RefreshHistory()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Обновление истории генерации...");
                LoadHistoryFromStorage();
                StatusMessage = $"📋 История обновлена ({GenerationHistory.Count} записей)";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления истории: {ex.Message}");
                StatusMessage = $"❌ Ошибка обновления: {ex.Message}";
            }
        }

        private void LoadHistoryFromStorage()
        {
            try
            {
                var historyPath = Path.Combine(FileSystem.AppDataDirectory, "GenerationHistory");
                if (!Directory.Exists(historyPath))
                {
                    Directory.CreateDirectory(historyPath);
                    return;
                }

                var files = Directory.GetFiles(historyPath, "*.json");
                var items = new List<GenerationHistoryItem>();

                foreach (var file in files)
                {
                    try
                    {
                        var json = File.ReadAllText(file);
                        var item = System.Text.Json.JsonSerializer.Deserialize<GenerationHistoryItem>(json);
                        if (item != null)
                        {
                            var imagePath = Path.Combine(historyPath, $"{item.Id}.png");
                            if (File.Exists(imagePath))
                            {
                                item.ImageBytes = File.ReadAllBytes(imagePath);
                                System.Diagnostics.Debug.WriteLine($"Загружено изображение для {item.Id}: {item.ImageBytes.Length} байт");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"Изображение не найдено для {item.Id}");
                            }
                            items.Add(item);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка загрузки файла {file}: {ex.Message}");
                    }
                }

                GenerationHistory.Clear();
                foreach (var item in items.OrderByDescending(i => i.Timestamp))
                {
                    GenerationHistory.Add(item);
                }

                System.Diagnostics.Debug.WriteLine($"Загружено {GenerationHistory.Count} записей из истории");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки истории: {ex.Message}");
            }
        }

        private void SaveHistoryToStorage(GenerationHistoryItem item)
        {
            try
            {
                var historyPath = Path.Combine(FileSystem.AppDataDirectory, "GenerationHistory");
                if (!Directory.Exists(historyPath))
                {
                    Directory.CreateDirectory(historyPath);
                }

                var jsonPath = Path.Combine(historyPath, $"{item.Id}.json");
                var json = System.Text.Json.JsonSerializer.Serialize(item, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(jsonPath, json);

                if (item.ImageBytes != null && item.ImageBytes.Length > 0)
                {
                    var imagePath = Path.Combine(historyPath, $"{item.Id}.png");
                    File.WriteAllBytes(imagePath, item.ImageBytes);
                }

                System.Diagnostics.Debug.WriteLine($"Сохранена запись: {item.Id}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения истории: {ex.Message}");
            }
        }

        private void LoadHistoryItem(GenerationHistoryItem? item)
        {
            if (item == null) return;

            System.Diagnostics.Debug.WriteLine($"Загрузка элемента истории: {item.Id}");

            if (!item.HasImage)
            {
                try
                {
                    var historyPath = Path.Combine(FileSystem.AppDataDirectory, "GenerationHistory");
                    var imagePath = Path.Combine(historyPath, $"{item.Id}.png");
                    if (File.Exists(imagePath))
                    {
                        item.ImageBytes = File.ReadAllBytes(imagePath);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка загрузки изображения: {ex.Message}");
                }
            }

            SelectedHistoryItem = item;
            Prompt = item.Prompt;
            HasImage = item.HasImage;
            StatusMessage = $"🖼️ Загружен: {item.Prompt}";

            if (IsHistoryVisible)
            {
                ToggleHistory();
            }
        }

        private async void ClearAllHistory()
        {
            try
            {
                if (GenerationHistory.Count == 0)
                {
                    StatusMessage = "📭 История уже пуста";
                    await Application.Current!.MainPage!.DisplayAlert("Информация", "История уже пуста", "ОК");
                    return;
                }

                bool answer = await Application.Current!.MainPage!.DisplayAlert(
                    "Очистка истории",
                    $"Вы уверены, что хотите удалить все {GenerationHistory.Count} записей?\nЭто действие нельзя отменить!",
                    "Да, удалить всё",
                    "Отмена"
                );

                if (!answer) return;

                var historyPath = Path.Combine(FileSystem.AppDataDirectory, "GenerationHistory");
                int deletedCount = 0;

                if (Directory.Exists(historyPath))
                {
                    var files = Directory.GetFiles(historyPath);
                    foreach (var file in files)
                    {
                        try
                        {
                            File.Delete(file);
                            deletedCount++;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Ошибка удаления {file}: {ex.Message}");
                        }
                    }
                }

                GenerationHistory.Clear();
                SelectedHistoryItem = null;
                HasImage = false;
                StatusMessage = $"🧹 Удалено {deletedCount} записей";

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
            }
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
                    StatusMessage = "Готов к работе!";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Ошибка: {ex.Message}";
                }
            }
            else
            {
                StatusMessage = "Ключ API не найден.";
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
                System.Diagnostics.Debug.WriteLine($"Запрос: {fullPrompt}");

                var response = await _gigachatService.SendMessageAsync(fullPrompt);
                System.Diagnostics.Debug.WriteLine($"Ответ: {response.Substring(0, Math.Min(200, response.Length))}...");

                var fileId = ExtractFileId(response);
                if (string.IsNullOrEmpty(fileId))
                {
                    StatusMessage = "В ответе нет ID изображения.";
                    HasImage = false;
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Найден ID файла: {fileId}");

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
                    Style = SelectedStyle,
                    Resolution = SelectedResolution,
                    ImageBytes = imageBytes,
                    Timestamp = DateTime.Now
                };

                SaveHistoryToStorage(historyItem);

                GenerationHistory.Insert(0, historyItem);
                SelectedHistoryItem = historyItem;
                HasImage = true;
                StatusMessage = $"✅ Изображение сгенерировано! ({imageBytes.Length / 1024} КБ)";

                int tokens = 0;
                try
                {
                    var tokenInfo = await _gigachatService.CountTokensAsync(fullPrompt, response);
                    if (tokenInfo != null)
                    {
                        tokens = tokenInfo.TotalTokens;
                        System.Diagnostics.Debug.WriteLine($"📊 Токенов на генерацию: {tokens}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Ошибка подсчёта токенов для генерации: {ex.Message}");
                }

                _statsService.AddImage(tokens);
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Ошибка: {ex.Message}";
                HasImage = false;
                System.Diagnostics.Debug.WriteLine($"Исключение: {ex}");
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
                System.Diagnostics.Debug.WriteLine($"Найден GUID: {guidMatch.Value}");
                return guidMatch.Value;
            }

            var hexPattern = @"[a-fA-F0-9]{32,}";
            var hexMatch = Regex.Match(message, hexPattern, RegexOptions.IgnoreCase);
            if (hexMatch.Success)
            {
                System.Diagnostics.Debug.WriteLine($"Найден HEX ID: {hexMatch.Value}");
                return hexMatch.Value;
            }

            System.Diagnostics.Debug.WriteLine($"ID не найден в ответе");
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
