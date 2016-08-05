using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Ba2Explorer.Utility;

namespace Ba2Explorer.View.Converters
{
    [ValueConversion(typeof(FilePathType), typeof(ImageSource))]
    public class FilePathTypeToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(ImageSource))
                throw new InvalidOperationException("The target must be a ImageSource.");

            FilePathType type = (FilePathType)value;
            switch (type) {
                case FilePathType.Directory:
                    return StockIcon.Folder;
                case FilePathType.File:
                    return StockIcon.AssociatedDocument;
                case FilePathType.GoBack:
                    return null;
                default:
                    throw new NotSupportedException($"{ type } is not supported.");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
