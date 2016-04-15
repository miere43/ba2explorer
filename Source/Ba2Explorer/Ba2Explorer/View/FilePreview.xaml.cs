using Ba2Explorer.ViewModel;
using S16.Drawing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Media;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ba2Explorer.View
{
    /// <summary>
    /// Control to preview various files from archive.
    /// </summary>
    public partial class FilePreview : UserControl
    {
        private readonly static IReadOnlyDictionary<string, string> extensionDescriptions;

        private ArchiveInfo archiveInfo;

        private string previewFilePath;

        enum FileType
        {
            Unknown,
            Text,
            Wav,
            Xml,
            Dds
        }

        public FilePreview()
        {
            InitializeComponent();
        }

        static FilePreview()
        {
            extensionDescriptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { ".pex", "Papyrus Compiled Script" },
                { ".psc", "Papyrus Script Source Code" },
                { ".nif", "Model Data" },
                { ".bgsm", "Material Data" },
                { ".bgem", "Material Data" },
                { ".hkx", "Havok Data" },
                { ".dlstrings", "Localization Strings" },
                { ".ilstrings", "Localization Strings" },
                { ".strings", "Localization Strings" },
                { ".swf", "Flash File" },
                { ".wav", "Sound File" },
                { ".xwm", "Sound File" },
                { ".lod", "LOD Terrain " },
                { ".btr", "Terrain LOD (NIF Model)" },
                { ".bto", "Object LOD (NIF Model)" },
                { ".bin", "Binary File" }
                // ssf
                // tri - trishapes?
                // hko - havok object?
                // obj
                // sclp - sculpt data?
                // log - text?
                // max
                // lst - tree lod info
                // vvd
                // dat
                // xwm - sound
                // gfx
                // uvd
                // fuz - voices?
            };
        }

        public void SetArchive(ArchiveInfo archive)
        {
            if (archive == null)
                throw new ArgumentNullException(nameof(archive));

            this.archiveInfo = archive;
            this.archiveInfo.PropertyChanged += ArchiveInfo_PropertyChanged;
            this.previewFilePath = null;
        }

        void ArchiveInfo_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Contract.Ensures(sender == archiveInfo);

            if (e.PropertyName == nameof(ArchiveInfo.IsDisposed) && archiveInfo.IsDisposed == true)
            {
                SetDefaultPreview();
                this.archiveInfo.PropertyChanged -= ArchiveInfo_PropertyChanged;
            }
        }

        public bool CanPreviewTarget(string filePath)
        {
            return ResolveFileTypeFromExtension(Path.GetExtension(filePath)) != FileType.Unknown;
        }

        public void SetUnknownPreviewTarget(string filePath)
        {
            EnsureArchiveAttached();

            if (String.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException(nameof(filePath));

            SetUnknownPreview(filePath);
        }

        public async Task<bool> TrySetPreviewAsync(string filePath)
        {
            EnsureArchiveAttached();

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
                    case FileType.Xml:
                    case FileType.Text:
                        SetTextPreview(stream);
                        break;
                    case FileType.Dds:
                        await SetDdsImagePreview(stream);
                        break;
                    case FileType.Wav:
                        SetWavSoundPreview(stream);
                        break;
                    default:
                       throw new NotSupportedException($"Preview of file with type \"{type}\" is not supported.");
                }
            }

            return true;
        }

        #region Private methods

        private void ChangeControlsVisibilityForFileType(FileType fileType)
        {
            if (fileType != FileType.Wav)
                SoundPlayerControl.StopSound();

            switch (fileType)
            {
                case FileType.Xml:
                case FileType.Text:
                case FileType.Unknown:
                    PreviewImageBox.Visibility = Visibility.Collapsed;
                    TextBlockScrollViewer.Visibility = Visibility.Visible;
                    SoundPlayerControl.Visibility = Visibility.Collapsed;
                    break;
                case FileType.Wav:
                    PreviewImageBox.Visibility = Visibility.Collapsed;
                    TextBlockScrollViewer.Visibility = Visibility.Collapsed;
                    SoundPlayerControl.Visibility = Visibility.Visible;
                    break;
                case FileType.Dds:
                    PreviewImageBox.Visibility = Visibility.Visible;
                    TextBlockScrollViewer.Visibility = Visibility.Collapsed;
                    SoundPlayerControl.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        /// <summary>
        /// Set's target text block text to text with grayed tooltip near it.
        /// </summary>
        /// <param name="textBlock">The TextBlock object.</param>
        /// <param name="text">The text.</param>
        /// <param name="tip">The tip.</param>
        private void SetTextWithTip(TextBlock textBlock, string text, string tip)
        {
            Contract.Requires(textBlock != null);
            Contract.Requires(text != null);
            Contract.Requires(tip != null);

            textBlock.Inlines.Clear();
            textBlock.Inlines.Add(text);
            Run grayedText = new Run(" (" + tip + ")");
            grayedText.Foreground = Brushes.Gray;
            textBlock.Inlines.Add(grayedText);
        }

        private void SetWavSoundPreview(Stream stream)
        {
            Contract.Requires(stream != null);

            stream.Seek(0, SeekOrigin.Begin);
            SoundPlayerControl.SoundSource = stream;

            ChangeControlsVisibilityForFileType(FileType.Wav);
        }

        /// <summary>
        /// Set's preview to DDS image.
        /// TODO: catch loading exceptions.
        /// </summary>
        /// <param name="stream">DDS image stream.</param>
        private async Task SetDdsImagePreview(Stream stream)
        {
            Contract.Requires(stream != null);
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

            this.PreviewImageBox.Source = source;
            ChangeControlsVisibilityForFileType(FileType.Dds);

            SetTextWithTip(this.PreviewText, "Preview",
                image.BitmapImage.Width + "x" + image.BitmapImage.Height);

            image.Dispose();
            Win32Util.DeleteObject(hBitmap);
        }

        /// <summary>
        /// Set's preview with no preview text.
        /// </summary>
        private void SetDefaultPreview()
        {
            this.PreviewText.Text = "Preview";
            this.PreviewImageBox.Source = null;
            this.PreviewTextField.Text = null;

            ChangeControlsVisibilityForFileType(FileType.Unknown);
        }

        /// <summary>
        /// Set's preview to text label with text that
        /// file previewer cannot show preview for
        /// file.
        /// </summary>
        private void SetUnknownPreview(string filePath)
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(filePath));

            string fileName = Path.GetFileName(filePath);
            string ext = Path.GetExtension(fileName);

            string desc = null;
            extensionDescriptions.TryGetValue(ext, out desc);

            SetTextWithTip(this.PreviewTextField,
                "Cannot preview " + Path.GetFileName(filePath), desc == null ? "unsupported" : desc);

            this.PreviewText.Text = "Preview";
            ChangeControlsVisibilityForFileType(FileType.Unknown);
        }

        /// <summary>
        /// Set's preview to text label with text
        /// readed from stream with ASCII encoding.
        /// </summary>
        private void SetTextPreview(Stream stream)
        {
            Contract.Requires(stream != null);

            byte[] buffer = new byte[stream.Length];
            int readed = stream.Read(buffer, 0, (int)stream.Length);
            Debug.Assert(readed == stream.Length);

            string text = Encoding.ASCII.GetString(buffer);

            this.PreviewTextField.Text = text;
            this.PreviewText.Text = "Preview";

            ChangeControlsVisibilityForFileType(FileType.Text);
        }

        /// <summary>
        /// Resolves extension to FileType enum.
        /// </summary>
        /// <param name="extension">Extension, can start with dot.</param>
        private FileType ResolveFileTypeFromExtension(string extension)
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(extension));

            extension = extension.TrimStart('.');

            if (extension.Equals("txt", StringComparison.OrdinalIgnoreCase))
            {
                return FileType.Text;
            }
            else if (extension.Equals("xml", StringComparison.OrdinalIgnoreCase))
            {
                return FileType.Xml;
            }
            else if (extension.Equals("wav", StringComparison.OrdinalIgnoreCase))
            {
                return FileType.Wav;
            }
            else if (extension.Equals("dds", StringComparison.OrdinalIgnoreCase))
            {
                return FileType.Dds;
            }

            return FileType.Unknown;
        }

        #endregion

        #region Helper methods

        void EnsureArchiveAttached()
        {
            if (archiveInfo == null || archiveInfo.IsDisposed)
                throw new InvalidOperationException("ArchiveInfo should be set with SetArchive() method.");
        }

        #endregion

    }
}
