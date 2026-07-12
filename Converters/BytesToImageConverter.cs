using System.Globalization;

namespace Giga.Converters
{
    public class BytesToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is byte[] imageBytes && imageBytes.Length > 0)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"🖼️ Конвертация: {imageBytes.Length} байт");
                    return ImageSource.FromStream(() => new MemoryStream(imageBytes));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Ошибка конвертации: {ex.Message}");
                    return null;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}