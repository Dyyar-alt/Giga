using Giga.Models;
using Giga.ViewModels;

#if ANDROID
using Android.Views.InputMethods;
using Android.Widget;
using Android.App;
#endif

namespace Giga.Views;

public partial class ChatPageNew : ContentPage
{
    private ChatViewModel? ViewModel => BindingContext as ChatViewModel;

    public ChatPageNew(ChatViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        this.Loaded += OnPageLoaded;
        this.Appearing += OnPageAppearing;
        this.Disappearing += OnPageDisappearing;

        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ChatViewModel.IsHistoryVisible))
        {
            AnimateHistoryPanel(ViewModel?.IsHistoryVisible ?? false);
        }
        else if (e.PropertyName == nameof(ChatViewModel.CurrentSessionMessages))
        {
            // Прокручиваем вниз при добавлении сообщений
            ScrollToBottom();
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

    private void OnPageLoaded(object? sender, EventArgs e)
    {
        FocusMessageEntry();
        ScrollToBottom();

        if (ViewModel?.IsHistoryVisible == true)
        {
            ViewModel.ToggleHistoryCommand?.Execute(null);
        }
    }

    private void OnPageAppearing(object? sender, EventArgs e)
    {
        FocusMessageEntry();
        ScrollToBottom();

        if (ViewModel != null && SessionsListView != null)
        {
            SessionsListView.ItemsSource = ViewModel.SavedSessions;
            ViewModel.RefreshHistoryCommand?.Execute(null);
        }
    }

    private void OnPageDisappearing(object? sender, EventArgs e)
    {
        if (ViewModel?.IsHistoryVisible == true)
        {
            ViewModel.ToggleHistoryCommand?.Execute(null);
        }

        ViewModel?.SaveSessionCommand?.Execute(null);
        ViewModel?.Cleanup();
    }

    private void OnEntryCompleted(object sender, EventArgs e)
    {
        if (ViewModel?.SendCommand?.CanExecute(null) == true)
        {
            ViewModel.SendCommand.Execute(null);
        }
        FocusMessageEntry();
    }

    private void OnSessionTapped(object sender, TappedEventArgs e)
    {
        try
        {
            var frame = sender as Frame;
            if (frame == null) return;

            var session = frame.BindingContext as ChatSession;
            if (session == null) return;

            System.Diagnostics.Debug.WriteLine($"Двойной тап по сессии: {session.Id}, сообщений: {session.Messages.Count}");

            ViewModel?.ToggleHistoryCommand?.Execute(null);

            Device.StartTimer(TimeSpan.FromMilliseconds(350), () =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        ViewModel?.LoadSessionCommand?.Execute(session);
                        ScrollToBottom();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка загрузки сессии: {ex.Message}");
                    }
                });
                return false;
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка в OnSessionTapped: {ex.Message}");
        }
    }

    private async void FocusMessageEntry()
    {
        try
        {
            await Task.Delay(300);

            if (MessageEntry != null)
            {
                MessageEntry.Focus();

#if ANDROID
                try
                {
                    var editText = MessageEntry.Handler?.PlatformView as EditText;
                    if (editText != null)
                    {
                        editText.RequestFocus();
                        var inputMethodManager = global::Android.App.Application.Context.GetSystemService(global::Android.App.Activity.InputMethodService) as InputMethodManager;
                        inputMethodManager?.ShowSoftInput(editText, ShowFlags.Implicit);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка клавиатуры: {ex.Message}");
                }
#endif
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка фокусировки: {ex.Message}");
        }
    }

    private async void ScrollToBottom()
    {
        try
        {
            await Task.Delay(100);

            if (MessagesListView != null && MessagesListView.ItemsSource != null)
            {
                var lastItem = MessagesListView.ItemsSource.Cast<object>().LastOrDefault();
                if (lastItem != null)
                {
                    MessagesListView.ScrollTo(lastItem, ScrollToPosition.End, true);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка прокрутки: {ex.Message}");
        }
    }

    protected override bool OnBackButtonPressed()
    {
        if (ViewModel?.IsHistoryVisible == true)
        {
            ViewModel.ToggleHistoryCommand?.Execute(null);
            return true;
        }

#if ANDROID
        try
        {
            var editText = MessageEntry?.Handler?.PlatformView as EditText;
            if (editText != null)
            {
                var inputMethodManager = global::Android.App.Application.Context.GetSystemService(global::Android.App.Activity.InputMethodService) as InputMethodManager;
                inputMethodManager?.HideSoftInputFromWindow(editText.WindowToken, 0);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка скрытия клавиатуры: {ex.Message}");
        }
#endif

        return base.OnBackButtonPressed();
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (height < 500)
        {
            ScrollToBottom();
        }
    }
}