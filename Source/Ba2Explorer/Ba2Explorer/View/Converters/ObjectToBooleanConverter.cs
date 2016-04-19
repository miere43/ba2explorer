using System;
using System.Windows.Data;

namespace Ba2Explorer.View.Converters
{
    /// <summary>
    /// Returns true when object is not null, false otherwise.
    /// </summary>
    [ValueConversion(typeof(object), typeof(object))]
    public class ObjectToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
