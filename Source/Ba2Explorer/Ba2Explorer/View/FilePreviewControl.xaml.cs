using Ba2Explorer.Utility;
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
using Ba2Explorer.Controller;
using System.ComponentModel;
using Ba2Explorer.Controls;
using System.Linq;

namespace Ba2Explorer.View
{

    /// <summary>
    /// Control to preview various files from archive.
    /// </summary>
    public partial class FilePreviewControl : UserControl
    {
        private static IReadOnlyDictionary<string, string> extensionDescriptions;

        private static object staticInitLock = new object();

        /// <summary>
        /// Lock to prevent access to `m_queueNextFileIndex` by UI and `m_previewWorker` threads simultaneously.
        /// </summary>
        private object m_queueLock = new object();

        private string m_queueFileName = null;

        //private FontFamily defaultTextFontFamily;

        //private FontFamily textFileFontFamily;

        private ArchiveInfo m_archive;

        /// <summary>
        /// Worker that decodes preview file data in background thread.
        /// </summary>
        private BackgroundWorker m_previewWorker;

        public FastTextBlock PreviewTextField;

        public FilePreviewControl()
        {
            LazyStaticInit();

            InitializeComponent();

            PreviewTextField = new FastTextBlock();
            PreviewTextFieldParent.Content = PreviewTextField;

            m_previewWorker = new BackgroundWorker();
            m_previewWorker.DoWork += LoadPreviewInBackground;
            m_previewWorker.RunWorkerCompleted += LoadPreviewCompleted;
        }

        private void LoadPreviewCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            PreviewFileData data = (PreviewFileData)e.Result;
            switch (data.FileType)
            {
                case PreviewFileType.Text:
                    SetTextPreview(data.TakeTextArgs());
                    break;
                case PreviewFileType.Dds:
                    SetDdsPreview(data.TakeDdsArgs());
                    break;
                case PreviewFileType.Wav:
                    SetWavSoundPreview(data.TakeWavArgs());
                    break;
                default:
                    SetUnknownPreview(m_archive.Archive.FileList.ElementAt(data.FileIndex));
                    break;
            }

            lock (m_queueLock)
            {
                if (m_queueFileName != null)
                {
                    var fileIndex = m_queueFileName;
                    m_queueFileName = null;
                    SetPreview(fileIndex);
                }
            }
        }

        private void LoadPreviewInBackground(object sender, DoWorkEventArgs args)
        {
            args.Result = FilePreviewer.LoadPreview(args);
        }

        ~FilePreviewControl()
        {
            DetachArchive();
            m_previewWorker.DoWork -= LoadPreviewInBackground;
            m_previewWorker.RunWorkerCompleted -= LoadPreviewCompleted;
        }

        #region Public methods

        public bool HasFileInQueue()
        {
            lock (m_queueLock)
            {
                return m_queueFileName != null;
            }
        }

        public void AttachArchive(ArchiveInfo archive)
        {
            if (archive == null)
                throw new ArgumentNullException(nameof(archive));

            m_archive = archive;
        }

        public void SetUnknownPreviewTarget(string filePath)
        {
            EnsureArchiveAttached();

            if (String.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException(nameof(filePath));

            SetUnknownPreview(filePath);
        }

        private void SetQueueFileName(string fileName)
        {
            lock (m_queueLock)
            {
                //Debug.WriteLine($"Setting queue item to {fileIndex}");
                m_queueFileName = fileName;
            }
        }
        
        private string GetQueueFileName()
        {
            lock (m_queueLock)
            {
                return m_queueFileName;
            }
        }

        /// <summary>
        /// Tries to set preview for selected file in archive.
        /// </summary>
        /// <param name="filePath">Path to file from archive.</param>
        public void SetPreview(string fileName)
        {
            if (m_archive == null || m_archive.IsDisposed)
                return;

            if (!IsEnabled || !m_archive.Archive.ContainsFile(fileName))
                return; // TODO set error preview?

            int fileIndex = m_archive.Archive.GetFileIndex(fileName);
            if (fileIndex == -1)
                return; // TODO set error preview?

            PreviewFileType fileType = FilePreviewer.ResolveFileTypeFromFileName(fileName);
            // TODO don't run previewWorker if fileType is unknown for better perf.

            if (m_previewWorker.IsBusy)
            {
                SetQueueFileName(fileName);
            }
            else
            {
                m_previewWorker.RunWorkerAsync(new object[] { m_archive, fileIndex, fileType });
            }
        }

        #endregion

        #region Private methods

        private void ChangeControlsVisibilityForFileType(PreviewFileType fileType)
        {
            if (fileType != PreviewFileType.Wav)
                SoundPlayerControl.StopAudio();

            switch (fileType)
            {
                case PreviewFileType.Text:
                case PreviewFileType.Unknown:
                    PreviewImageBox.Visibility = Visibility.Collapsed;
                    PreviewTextFieldParent.Visibility = Visibility.Visible;
                    SoundPlayerControl.Visibility = Visibility.Collapsed;
                    break;
                case PreviewFileType.Wav:
                    PreviewImageBox.Visibility = Visibility.Collapsed;
                    PreviewTextFieldParent.Visibility = Visibility.Collapsed;
                    SoundPlayerControl.Visibility = Visibility.Visible;
                    break;
                case PreviewFileType.Dds:
                    PreviewImageBox.Visibility = Visibility.Visible;
                    PreviewTextFieldParent.Visibility = Visibility.Collapsed;
                    SoundPlayerControl.Visibility = Visibility.Collapsed;
                    break;
            }

            //if (fileType == PreviewFileType.Unknown)
            //    PreviewTextField.FontFamily = defaultTextFontFamily;
            //else
            //    PreviewTextField.FontFamily = textFileFontFamily;
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

            // textBlock.Text = text + "(" + tip + ")";

            textBlock.Inlines.Clear();
            textBlock.Inlines.Add(text);
            Run grayedText = new Run(" (" + tip + ')');
            grayedText.Foreground = Brushes.Gray;
            textBlock.Inlines.Add(grayedText);
        }

        private void SetWavSoundPreview(Stream stream)
        {
            Contract.Requires(stream != null);

            if (SoundPlayerControl.SoundSource != null)
                SoundPlayerControl.SoundSource.Dispose();

            stream.Seek(0, SeekOrigin.Begin);
            SoundPlayerControl.SoundSource = stream;

            ChangeControlsVisibilityForFileType(PreviewFileType.Wav);
        }

        /// <summary>
        /// Set's preview to DDS image.
        /// </summary>
        /// <param name="stream">DDS image stream.</param>
        private void SetDdsPreview(DdsImage image)
        {
            Contract.Requires(image != null);

            IntPtr hBitmap = image.BitmapImage.GetHbitmap();
            BitmapSource source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            this.PreviewImageBox.Source = source;

            NativeMethods.DeleteObject(hBitmap);
            image.Dispose();

            ChangeControlsVisibilityForFileType(PreviewFileType.Dds);

            SetTextWithTip(this.PreviewText, "Preview",
                source.PixelWidth + "x" + source.PixelHeight);
        }

        private void SetErrorPreview(string error)
        {
            Contract.Requires(error != null);

            this.PreviewTextField.Text = error;
            this.PreviewText.Text = "Preview";

            this.ChangeControlsVisibilityForFileType(PreviewFileType.Text);
        }

        /// <summary>
        /// Set's preview with no preview text.
        /// </summary>
        private void SetDefaultPreview()
        {
            this.PreviewText.Text = "Preview";
            this.PreviewImageBox.Source = null;
            this.PreviewTextField.Text = null;

            ChangeControlsVisibilityForFileType(PreviewFileType.Unknown);
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

            PreviewTextField.Text = "Cannot preview " + Path.GetFileName(filePath);
            //SetTextWithTip(this.PreviewTextField,
             //   "Cannot preview " + Path.GetFileName(filePath), desc == null ? "unsupported" : desc);

            this.PreviewText.Text = "Preview";
            ChangeControlsVisibilityForFileType(PreviewFileType.Unknown);
        }

        private void SetTextPreview(string text)
        {
            Contract.Requires(text != null);

                //Debug.WriteLine("text length: {0}", text.Length);
                PreviewTextField.Text = text;
                PreviewText.Text = "Preview";

                ChangeControlsVisibilityForFileType(PreviewFileType.Text);

            //Stopwatch w = new Stopwatch();
            //w.Start();
            //this.PreviewTextField.Text = text;
            //this.PreviewText.Text = "Preview";
            //w.Stop();
            //Debug.WriteLine("elapsed: {0}", w.ElapsedMilliseconds);
        }

        #endregion

        #region Helper methods

        private void DetachArchive()
        {
            if (m_archive == null)
                return;

            this.m_archive.PropertyChanged -= ArchiveInfo_PropertyChanged;
            this.m_archive = null;
        }

        /// <summary>
        /// Lazily initializes static properties of FilePreview class.
        /// </summary>
        private static void LazyStaticInit()
        {
            lock (staticInitLock)
            {
                if (extensionDescriptions != null)
                    return;

                extensionDescriptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { ".pex", "Papyrus Compiled Script" },
                    { ".psc", "Papyrus Script Source Code" },
                    { ".nif", "Model Data" },
                    { ".bgsm", "Material Data" },
                    { ".bgem", "Material Data" },
                    { ".hkx", "Havok Animation Data" },
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
        }

        private void ArchiveInfo_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Contract.Ensures(sender == m_archive);

            if (e.PropertyName == nameof(ArchiveInfo.IsDisposed) && m_archive.IsDisposed == true)
            {
                SetDefaultPreview();
                this.m_archive.PropertyChanged -= ArchiveInfo_PropertyChanged;
            }
        }

        private void EnsureArchiveAttached()
        {
            if (m_archive == null || m_archive.IsDisposed)
                throw new InvalidOperationException("ArchiveInfo should be set with SetArchive() method.");
        }

        #endregion
    }
}
