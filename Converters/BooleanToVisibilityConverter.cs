using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RecoilMacro.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                if (parameter is string p && p.Equals("Invert", StringComparison.OrdinalIgnoreCase))
                    return b ? Visibility.Collapsed : Visibility.Visible;
                return b ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility v) return v == Visibility.Visible;
            return false;
        }
    }
}
