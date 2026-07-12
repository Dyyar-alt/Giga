using Giga.Models;
using Giga.ViewModels;

namespace Giga.Views;

public partial class ImageGenPage : ContentPage
{
    private ImageGenerationViewModel? ViewModel => BindingContext as ImageGenerationViewModel;

    public ImageGenPage(ImageGenerationViewModel viewModel)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("ImageGenPage: Конструктор START");
            InitializeComponent();
            BindingContext = viewModel;

            viewModel.PropertyChanged += OnViewModelPropertyChanged;

            System.Diagnostics.Debug.WriteLine("ImageGenPage: Конструктор END");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ImageGenPage: Ошибка: {ex.Message}");
            throw;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ImageGenerationViewModel.IsHistoryVisible))
        {
            AnimateHistoryPanel(ViewModel?.IsHistoryVisible ?? false);
        }
        else if (e.PropertyName == nameof(ImageGenerationViewModel.GenerationHistory))
        {
            // Обновляем список при добавлении
            if (HistoryListView != null && ViewModel != null)
            {
                HistoryListView.ItemsSource = null;
                HistoryListView.ItemsSource = ViewModel.GenerationHistory;
            }
        }
    }

    private async void AnimateHistoryPanel(bool show)
    {
        try
        {
            if (show)
            {
                HistoryPanel.IsVisible = true;
                Overlay.IsVisible = true;
                await HistoryPanel.TranslateTo(350, 0, 0);
                await HistoryPanel.FadeTo(1, 250);
                await HistoryPanel.TranslateTo(0, 0, 300, Easing.CubicOut);
            }
            else
            {
                await HistoryPanel.TranslateTo(350, 0, 300, Easing.CubicIn);
                await HistoryPanel.FadeTo(0, 250);
                HistoryPanel.IsVisible = false;
                Overlay.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка анимации: {ex.Message}");
            HistoryPanel.IsVisible = show;
            Overlay.IsVisible = show;
            HistoryPanel.Opacity = show ? 1 : 0;
        }
    }

    private void OnHistoryItemTapped(object sender, TappedEventArgs e)
    {
        try
        {
            var frame = sender as Frame;
            if (frame == null) return;

            var item = frame.BindingContext as GenerationHistoryItem;
            if (item == null) return;

            System.Diagnostics.Debug.WriteLine($"Двойной тап по истории: {item.Id}");

            ViewModel?.ToggleHistoryCommand?.Execute(null);

            Device.StartTimer(TimeSpan.FromMilliseconds(350), () =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        ViewModel?.LoadHistoryItemCommand?.Execute(item);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка загрузки: {ex.Message}");
                    }
                });
                return false;
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка в OnHistoryItemTapped: {ex.Message}");
        }
    }

    protected override void OnAppearing()
    {
        try
        {
            base.OnAppearing();
            System.Diagnostics.Debug.WriteLine("ImageGenPage: OnAppearing");
            ViewModel?.RefreshHistoryCommand?.Execute(null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ImageGenPage: Ошибка в OnAppearing: {ex.Message}");
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
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ImageGenPage: Ошибка в OnDisappearing: {ex.Message}");
        }
    }

    protected override bool OnBackButtonPressed()
    {
        if (ViewModel?.IsHistoryVisible == true)
        {
            ViewModel.ToggleHistoryCommand?.Execute(null);
            return true;
        }
        return base.OnBackButtonPressed();
    }
}