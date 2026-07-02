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

        // ✅ Подписываемся на события
        this.Loaded += OnPageLoaded;
        this.Appearing += OnPageAppearing;
    }

    private void OnPageLoaded(object? sender, EventArgs e)
    {
        // ✅ Устанавливаем фокус на поле ввода
        FocusMessageEntry();
        ScrollToBottom();
    }

    private void OnPageAppearing(object? sender, EventArgs e)
    {
        // ✅ При появлении страницы фокусируем поле ввода
        FocusMessageEntry();
        ScrollToBottom();
    }

    private void OnEntryCompleted(object sender, EventArgs e)
    {
        if (ViewModel?.SendCommand?.CanExecute(null) == true)
        {
            ViewModel.SendCommand.Execute(null);
        }
        // ✅ После отправки снова фокусируем поле
        FocusMessageEntry();
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel == null) return;

        var selectedSession = e.CurrentSelection.FirstOrDefault() as ChatSession;
        if (selectedSession == null) return;

        ViewModel.LoadSessionCommand?.Execute(selectedSession);
        ((CollectionView)sender).SelectedItem = null;

        ScrollToBottom();
        FocusMessageEntry();
    }

    // ✅ Метод для фокусировки поля ввода
    private async void FocusMessageEntry()
    {
        try
        {
            // Небольшая задержка для рендеринга
            await Task.Delay(300);

            if (MessageEntry != null)
            {
                // Устанавливаем фокус
                MessageEntry.Focus();

                // На Android вызываем клавиатуру
#if ANDROID
                try
                {
                    var editText = MessageEntry.Handler?.PlatformView as EditText;
                    if (editText != null)
                    {
                        editText.RequestFocus();
                        // ✅ Используем global:: для разрешения конфликта имен
                        var inputMethodManager = global::Android.App.Application.Context.GetSystemService(global::Android.App.Activity.InputMethodService) as InputMethodManager;
                        inputMethodManager?.ShowSoftInput(editText, ShowFlags.Implicit);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка показа клавиатуры: {ex.Message}");
                }
#endif
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка фокусировки: {ex.Message}");
        }
    }

    // ✅ Метод для прокрутки вниз
    private async void ScrollToBottom()
    {
        try
        {
            await Task.Delay(100);

            if (MessagesScrollView != null)
            {
                await MessagesScrollView.ScrollToAsync(0, int.MaxValue, true);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка прокрутки: {ex.Message}");
        }
    }

    // ✅ Обработка нажатия на клавишу Back на Android
    protected override bool OnBackButtonPressed()
    {
#if ANDROID
        try
        {
            // Скрываем клавиатуру при нажатии Back
            var editText = MessageEntry?.Handler?.PlatformView as EditText;
            if (editText != null)
            {
                // ✅ Используем global:: для разрешения конфликта имен
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

        // При изменении размера (появление клавиатуры) прокручиваем вниз
        if (height < 500)
        {
            ScrollToBottom();
        }
    }
}