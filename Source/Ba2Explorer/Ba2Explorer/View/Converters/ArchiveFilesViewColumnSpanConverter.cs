using System;
using System.Windows;
using System.Windows.Data;

namespace Ba2Explorer.View.Converters
{
    [ValueConversion(typeof(bool), typeof(int))]
    public class ArchiveFilesViewColumnSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(int))
                throw new InvalidOperationException("The target must be a int.");

            return (bool)value == true ? 1 : 2;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
