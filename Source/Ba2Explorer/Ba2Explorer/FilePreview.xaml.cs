using S16.Drawing;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ba2Explorer
{
    public partial class FilePreview : UserControl
    {
        private enum FileType
        {
            Unknown,
            Text,
            DdsImage
        }

        public FilePreview()
        {
            InitializeComponent();
        }

        public bool CanPreviewTarget(string filePath)
        {
            return ResolveFileTypeFromExtension(Path.GetExtension(filePath)) != FileType.Unknown;
        }

        public void SetUnknownPreviewTarget(string filePath)
        {
            if (String.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException(nameof(filePath));

            SetUnknownPreview(filePath);
        }

        public void SetPreviewTarget(Stream stream, string filePath)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (String.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException(nameof(filePath));

            FileType fileType = ResolveFileTypeFromExtension(Path.GetExtension(filePath));
            switch (fileType)
            {
                case FileType.Unknown:
                    SetUnknownPreview(filePath);
                    break;
                case FileType.Text:
                    SetTextPreview(stream);
                    break;
                case FileType.DdsImage:
                    SetDdsImagePreview(stream);
                    break;
                default:
                    throw new NotSupportedException($"Preview of file with type \"{fileType}\" is not supported.");
            }
        }

        private void SetDdsImagePreview(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);

            DdsImage image = new DdsImage(stream);
            if (!image.IsValid)
            {
                image.Dispose();
                // TODO
                return;
            }

            IntPtr hBitmap = image.BitmapImage.GetHbitmap();
            var source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            image.Dispose();
            Win32Util.DeleteObject(hBitmap);

            this.PreviewImageBox.Source = source;
            this.PreviewImageBox.Visibility = Visibility.Visible;
            this.PreviewTextField.Visibility = Visibility.Collapsed;
        }

        private void SetUnknownPreview(string filePath)
        {
            this.PreviewTextField.Inlines.Clear();
            this.PreviewTextField.Inlines.Add("Cannot preview " + Path.GetFileName(filePath));
            var grayedText = new Run(" (unsupported)");
            grayedText.Foreground = Brushes.Gray;
            this.PreviewTextField.Inlines.Add(grayedText);

            this.PreviewImageBox.Visibility = Visibility.Collapsed;
            this.PreviewTextField.Visibility = Visibility.Visible;
        }

        private void SetTextPreview(Stream stream)
        {
            byte[] buffer = new byte[stream.Length];
            int readed = stream.Read(buffer, 0, (int)stream.Length);
            Debug.Assert(readed == stream.Length);

            string text = Encoding.ASCII.GetString(buffer);

            this.PreviewTextField.Text = text;
            this.PreviewImageBox.Visibility = Visibility.Collapsed;
            this.PreviewTextField.Visibility = Visibility.Visible;
        }

        private FileType ResolveFileTypeFromExtension(string extension)
        {
            extension = extension.TrimStart('.');

            if (extension.Equals("txt", StringComparison.OrdinalIgnoreCase))
            {
                return FileType.Text;
            }
            else if (extension.Equals("dds", StringComparison.OrdinalIgnoreCase))
            {
                return FileType.DdsImage;
            }

            return FileType.Unknown;
        }
    }
}
