using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PocketCat.Helpers
{
    public class ColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string colorString)
            {
                try
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorString));
                }
                catch
                {
                    // Return a fallback color in case of an invalid string
                    return new SolidColorBrush(Colors.Black);
                }
            }

            return new SolidColorBrush(Colors.Black); // Default fallback
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
