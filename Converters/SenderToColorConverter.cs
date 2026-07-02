using System.Globalization;

namespace Giga.Converters
{
    public class SenderToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string sender)
            {
                return sender == "User" ? Color.FromHex("#3465A4") : Color.FromHex("#404040");
            }
            return Color.FromHex("#808080");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}