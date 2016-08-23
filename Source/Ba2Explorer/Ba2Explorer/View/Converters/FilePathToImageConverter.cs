using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using Ba2Explorer.Utility;

namespace Ba2Explorer.View.Converters
{
    [ValueConversion(typeof(FilePathType), typeof(ImageSource))]
    public class FilePathToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(ImageSource))
                throw new InvalidOperationException("The target must be a ImageSource.");

            ArchiveFilePath filePath = (ArchiveFilePath)value;
            switch (filePath.Type) {
                case FilePathType.Directory:
                    return StockIcon.Folder;
                case FilePathType.File:
                    return IconCache.GetSmallIconFromExtension(Path.GetExtension(filePath.DisplayPath));
                default:
                    throw new NotSupportedException($"{ filePath } is not supported.");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
