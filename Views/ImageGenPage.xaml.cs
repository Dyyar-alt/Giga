using Giga.ViewModels;
using System.Windows.Input;

namespace Giga.Views;

public partial class ImageGenPage : ContentPage
{
    private ImageGenerationViewModel? ViewModel => BindingContext as ImageGenerationViewModel;
    private CancellationTokenSource? _gifAnimationCts;

    public ImageGenPage(ImageGenerationViewModel viewModel)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("✅ ImageGenPage: Конструктор START");
            InitializeComponent();
            BindingContext = viewModel;

            SetupBindings();

            if (ViewModel != null)
            {
                ViewModel.PropertyChanged += OnViewModelPropertyChanged;
                UpdatePreviewImage(ViewModel.SelectedHistoryItem);
                UpdateSaveButtons(ViewModel.SelectedHistoryItem);
            }

            System.Diagnostics.Debug.WriteLine("✅ ImageGenPage: Конструктор END");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ ImageGenPage: Ошибка: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ Stack: {ex.StackTrace}");
            throw;
        }
    }

    private void SetupBindings()
    {
        try
        {
            if (HistoryCollectionView != null && ViewModel != null)
            {
                HistoryCollectionView.SetBinding(CollectionView.ItemsSourceProperty, nameof(ImageGenerationViewModel.GenerationHistory));
                HistoryCollectionView.SetBinding(CollectionView.SelectedItemProperty, new Binding(nameof(ImageGenerationViewModel.SelectedHistoryItem), BindingMode.TwoWay));
                HistoryCollectionView.SelectionChanged += OnHistorySelectionChanged;
            }

            if (SaveButtonsGrid != null && ViewModel != null)
            {
                var pngButton = SaveButtonsGrid.Children[0] as Button;
                var jpgButton = SaveButtonsGrid.Children[1] as Button;
                var gifButton = SaveButtonsGrid.Children[2] as Button;

                if (pngButton != null)
                {
                    pngButton.SetBinding(Button.CommandProperty, nameof(ImageGenerationViewModel.SaveImageCommand));
                    pngButton.SetBinding(Button.CommandParameterProperty, nameof(ImageGenerationViewModel.SelectedHistoryItem));
                }

                if (jpgButton != null)
                {
                    jpgButton.SetBinding(Button.CommandProperty, nameof(ImageGenerationViewModel.SaveImageWithPickerCommand));
                    jpgButton.SetBinding(Button.CommandParameterProperty, nameof(ImageGenerationViewModel.SelectedHistoryItem));
                }

                if (gifButton != null)
                {
                    gifButton.SetBinding(Button.CommandProperty, nameof(ImageGenerationViewModel.SaveImageWithPickerCommand));
                    gifButton.SetBinding(Button.CommandParameterProperty, nameof(ImageGenerationViewModel.SelectedHistoryItem));
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка настройки привязок: {ex.Message}");
        }
    }

    private void OnHistorySelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (ViewModel?.SelectedHistoryItem != null)
            {
                UpdatePreviewImage(ViewModel.SelectedHistoryItem);
                UpdateSaveButtons(ViewModel.SelectedHistoryItem);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка выбора истории: {ex.Message}");
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        try
        {
            if (e.PropertyName == nameof(ImageGenerationViewModel.SelectedHistoryItem))
            {
                UpdatePreviewImage(ViewModel?.SelectedHistoryItem);
                UpdateSaveButtons(ViewModel?.SelectedHistoryItem);
            }
            else if (e.PropertyName == nameof(ImageGenerationViewModel.IsGenerating))
            {
                // Запускаем или останавливаем анимацию GIF
                if (ViewModel?.IsGenerating == true)
                {
                    StartGifAnimation();
                }
                else
                {
                    StopGifAnimation();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка изменения свойства: {ex.Message}");
        }
    }

    private async void StartGifAnimation()
    {
        StopGifAnimation();
        _gifAnimationCts = new CancellationTokenSource();

        try
        {
            var gifImage = this.FindByName<Image>("brain.gif");
            if (gifImage == null) return;

            // Простой способ анимации - обновляем источник каждые 100ms
            while (!_gifAnimationCts.Token.IsCancellationRequested)
            {
                // Принудительно обновляем изображение
                var currentSource = gifImage.Source;
                gifImage.Source = null;
                await Task.Delay(1);
                gifImage.Source = currentSource;
                await Task.Delay(100, _gifAnimationCts.Token);
            }
        }
        catch (TaskCanceledException)
        {
            // Ожидаемо
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка анимации GIF: {ex.Message}");
        }
    }

    private void StopGifAnimation()
    {
        _gifAnimationCts?.Cancel();
        _gifAnimationCts?.Dispose();
        _gifAnimationCts = null;
    }

    private void UpdatePreviewImage(GenerationHistoryItem? item)
    {
        try
        {
            if (item?.ImageBytes != null && item.ImageBytes.Length > 0 && PreviewImage != null)
            {
                PreviewImage.Source = ImageSource.FromStream(() => new MemoryStream(item.ImageBytes));
                System.Diagnostics.Debug.WriteLine("✅ Изображение обновлено");
            }
            else if (PreviewImage != null)
            {
                PreviewImage.Source = null;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка обновления изображения: {ex.Message}");
        }
    }

    private void UpdateSaveButtons(GenerationHistoryItem? item)
    {
        try
        {
            if (SaveButtonsGrid != null)
            {
                bool isVisible = item?.ImageBytes != null && item.ImageBytes.Length > 0;
                SaveButtonsGrid.IsVisible = isVisible;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка обновления кнопок: {ex.Message}");
        }
    }

    protected override void OnAppearing()
    {
        try
        {
            base.OnAppearing();
            System.Diagnostics.Debug.WriteLine("✅ ImageGenPage: OnAppearing");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ ImageGenPage: Ошибка в OnAppearing: {ex.Message}");
        }
    }

    protected override void OnDisappearing()
    {
        try
        {
            base.OnDisappearing();
            if (ViewModel != null)
            {
                ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            }
            StopGifAnimation();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ ImageGenPage: Ошибка в OnDisappearing: {ex.Message}");
        }
    }
}