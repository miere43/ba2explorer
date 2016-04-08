using Ba2Explorer.ViewModel;
using S16.Drawing;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ba2Explorer
{
    public partial class FilePreview : UserControl
    {
        private ArchiveInfo archiveInfo;

        private string previewFilePath;

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

        ~FilePreview()
        {

        }

        public void SetArchive(ArchiveInfo archive)
        {
            this.archiveInfo = archive;
            this.previewFilePath = null;
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

        public async Task<bool> TrySetPreviewAsync(string filePath)
        {
            if (String.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException(nameof(filePath));

            if (!this.IsEnabled || !archiveInfo.Contains(filePath))
                return false;

            this.previewFilePath = filePath;
            FileType type = ResolveFileTypeFromExtension(Path.GetExtension(previewFilePath));

            if (type == FileType.Unknown)
            {
                SetUnknownPreview(filePath);
                return true;
            }

            using (MemoryStream stream = new MemoryStream())
            {
                await archiveInfo.ExtractToStreamAsync(filePath, stream);

                switch (type)
                {
                    case FileType.Text:
                        SetTextPreview(stream);
                        break;
                    case FileType.DdsImage:
                        await SetDdsImagePreview(stream);
                        break;
                    case FileType.Unknown:
                       throw new InvalidOperationException();
                    default:
                       throw new NotSupportedException($"Preview of file with type \"{type}\" is not supported.");
                }
            }

            return true;
        }

        private async Task SetDdsImagePreview(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);

             DdsImage image = await DdsImage.LoadAsync(stream);
            if (!image.IsValid)
            {
                image.Dispose();
                return;
            }

            IntPtr hBitmap = image.BitmapImage.GetHbitmap();
            BitmapSource source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
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
