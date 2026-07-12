namespace Giga.Behaviors
{
    public class GifAnimationBehavior : Behavior<Image>
    {
        private Image _image;
        private CancellationTokenSource _cts;
        private bool _isAnimating;

        protected override void OnAttachedTo(Image bindable)
        {
            base.OnAttachedTo(bindable);
            _image = bindable;
            _image.PropertyChanged += OnImagePropertyChanged;
        }

        protected override void OnDetachingFrom(Image bindable)
        {
            base.OnDetachingFrom(bindable);
            _image.PropertyChanged -= OnImagePropertyChanged;
            StopAnimation();
        }

        private async void OnImagePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Image.IsVisible) || e.PropertyName == nameof(Image.Source))
            {
                if (_image.IsVisible && _image.Source != null)
                {
                    await StartAnimationAsync();
                }
                else
                {
                    StopAnimation();
                }
            }
        }

        private async Task StartAnimationAsync()
        {
            if (_isAnimating || _image.Source == null)
                return;

            StopAnimation();

            _cts = new CancellationTokenSource();
            _isAnimating = true;

            try
            {
                while (!_cts.Token.IsCancellationRequested && _image.IsVisible)
                {
                    // Принудительно обновляем Image для анимации GIF
                    var source = _image.Source;
                    _image.Source = null;
                    await Task.Delay(1, _cts.Token);
                    _image.Source = source;
                    await Task.Delay(100, _cts.Token);
                }
            }
            catch (TaskCanceledException)
            {
                // Ожидаемое исключение при отмене
            }
            finally
            {
                _isAnimating = false;
            }
        }

        private void StopAnimation()
        {
            _isAnimating = false;
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
    }
}