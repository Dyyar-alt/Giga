using System.Globalization;

namespace Giga.Converters
{
    public class WidthToMaxWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double width && width > 0)
            {
                // Устанавливаем ширину ответа бота в 85% от ширины страницы для заполнения
                var maxWidth = width * 0.85;

                // Ограничиваем:
                // - на телефонах (ширина ~400px) — не меньше 320
                // - на десктопах — не больше 850
                if (maxWidth < 320) return 320;
                if (maxWidth > 850) return 850;
                return maxWidth;
            }
            return 350; // По умолчанию
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
