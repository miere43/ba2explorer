using GalaSoft.MvvmLight;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.IO;
using System;
using Microsoft.Win32;
using Ba2Explorer.View;
using Ba2Explorer.Logging;
using Ba2Explorer.Settings;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ba2Explorer.Utility;
using System.Linq;

namespace Ba2Explorer.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        public sealed class ExtractionEventArgs : EventArgs
        {
            public readonly string ExtractedFileName;

            public readonly int ExtractedCount;

            public readonly string DestinationFolder;

            public readonly ExtractionFinishedState State;

            public ExtractionEventArgs(string extractedFileName, int extractedCount, string destFolder, ExtractionFinishedState state)
            {
                ExtractedFileName = extractedFileName;
                ExtractedCount = extractedCount;
                DestinationFolder = destFolder;
                State = state;
            }
        }

        #region Properties 

        private ArchiveInfo archiveInfo;
        public ArchiveInfo ArchiveInfo
        {
            get
            {
                return archiveInfo;
            }
            private set
            {
                archiveInfo = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<string> recentArchives;
        public ObservableCollection<string> RecentArchives
        {
            get { return recentArchives; }
            set
            {
                recentArchives = value;
                RaisePropertyChanged();
            }
        }

        public bool HasRecentArchives
        {
            get
            {
                return RecentArchives != null && RecentArchives.Count > 0;
            }
        }

        #endregion

        public event EventHandler OnArchiveOpened;

        /// <summary>
        /// Called when archive was closed. True boolean value shows that UI should be resetted to default.
        /// </summary>
        public event EventHandler<bool> OnArchiveClosed;

        public event EventHandler<ExtractionEventArgs> OnExtractionCompleted;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            ArchiveInfo = null;

#if DEBUG
            if (IsInDesignMode)
                PrepareDesignTimeData();
#endif

            RecentArchives = AppSettings.Instance.MainWindow.GetLatestFiles();
            RecentArchives.CollectionChanged += (sender, args) => RaisePropertyChanged(nameof(HasRecentArchives));
        }

        /// <summary>
        /// Called by main window.
        /// </summary>
        public void MainWindowLoaded()
        {
            string[] cmdargs = Environment.GetCommandLineArgs();
            if (cmdargs.Length > 1)
            {
                if (cmdargs[1][0] != '/')
                {
                    OpenArchive(cmdargs[1]);
                }
            }
        }

        [Conditional("DEBUG")]
        private void PrepareDesignTimeData()
        {
        }

        /// <summary>
        /// Unregisters this instance from the Messenger class.
        /// <para>To cleanup additional resources, override this method, clean
        /// up and then call base.Cleanup().</para>
        /// </summary>
        public override void Cleanup()
        {
            if (ArchiveInfo != null && !ArchiveInfo.IsDisposed)
                ArchiveInfo.Dispose();

            base.Cleanup();
        }

        public void OpenArchiveWithDialog()
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Title = "Open archive";
            dialog.Filter = "BA2 Archives|*.ba2|All files|*.*";
            dialog.CheckPathExists = true;
            dialog.CheckFileExists = true;

            if (!String.IsNullOrWhiteSpace(AppSettings.Instance.Global.OpenArchiveLatestFolder))
                dialog.InitialDirectory = AppSettings.Instance.Global.OpenArchiveLatestFolder;

            dialog.ShowDialog();

            if (String.IsNullOrWhiteSpace(dialog.FileName))
                return;

            OpenArchive(dialog.FileName);
        }

        /// <summary>
        /// Opens the archive from file path.
        /// </summary>
        /// <exception cref="ArgumentException" />
        public bool OpenArchive(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
                throw new ArgumentException(nameof(path));

            // Requested same archive.
            if (archiveInfo != null)
                if (path.Equals(this.archiveInfo.FilePath, StringComparison.OrdinalIgnoreCase))
                    return false;

            if (ArchiveInfo != null)
            {
                CloseArchive(false);
            }

            try
            {
                ArchiveInfo = ArchiveInfo.Open(path);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Unable to open archive:{ Environment.NewLine }{ Environment.NewLine }{ e.Message }", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            AppSettings.Instance.Global.OpenArchiveLatestFolder = Path.GetDirectoryName(path);
            App.Logger.Log(LogPriority.Info, "Opened archive {0}", path);

            AppSettings.Instance.MainWindow.PushRecentArchive(path, this.RecentArchives);
            OnArchiveOpened?.Invoke(this, EventArgs.Empty);
            return true;
        }

        /// <summary>
        /// Closes archive and optionally changes application
        /// title back to normal.
        /// </summary>
        /// <param name="resetUI">Reset user interface?</param>
        public void CloseArchive(bool resetUI)
        {
            if (ArchiveInfo == null)
                return;

            if (!ArchiveInfo.IsDisposed)
            {
                ArchiveInfo.Dispose();
            }

            ArchiveInfo = null;
            OnArchiveClosed?.Invoke(this, resetUI);
        }

        /// <summary>
        /// Extracts file from archive to some file in user file system, asking him where to save file using SaveFileDialog.
        /// </summary>
        /// <param name="fileName">File name in archive.</param>
        /// <returns>Task.</returns>
        public async Task ExtractFileWithDialog(int fileIndex)
        {
            string fileName = ArchiveInfo.Archive.FileList.ElementAt(fileIndex);
            if (fileName == null)
                throw new Exception($"no file with index {fileIndex} exists.");

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.CheckPathExists = true;
            dialog.OverwritePrompt = true;
            dialog.ValidateNames = true;
            dialog.Title = "Extract file as...";
            dialog.FileName = LowerFileNameExtension(Path.GetFileName(fileName));

            if (!String.IsNullOrWhiteSpace(AppSettings.Instance.Global.ExtractionLatestFolder))
                dialog.InitialDirectory = AppSettings.Instance.Global.ExtractionLatestFolder;

            string ext = Path.GetExtension(dialog.SafeFileName).ToLower();
            dialog.Filter = "Specified file|*" + ext + "|All files|*.*";

            var result = dialog.ShowDialog();

            if (result.HasValue && result.Value == true)
            {
                await ArchiveInfo.ExtractFileAsync(fileIndex, dialog.FileName);
                string destFolder = Path.GetDirectoryName(dialog.FileName);
                AppSettings.Instance.Global.ExtractionLatestFolder = destFolder;

                OnExtractionCompleted?.Invoke(this, new ExtractionEventArgs(dialog.FileName, 1, destFolder,
                    ExtractionFinishedState.Succeed));
            }
        }

        public void ExtractAllWithDialog()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "Extract files to folder...";
            dialog.ShowNewFolderButton = true;

            if (!String.IsNullOrWhiteSpace(AppSettings.Instance.Global.ExtractionLatestFolder))
                dialog.SelectedPath = AppSettings.Instance.Global.ExtractionLatestFolder;

            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                ExtractAll(dialog.SelectedPath);
            }
            else { return; }
        }

        private void ExtractAll(string destinationFolder)
        {
            if (String.IsNullOrWhiteSpace(destinationFolder))
                throw new ArgumentException(nameof(destinationFolder));

            AppSettings.Instance.Global.ExtractionLatestFolder = Path.GetDirectoryName(destinationFolder);

            FileExtractionWindow window = new FileExtractionWindow();
            window.ViewModel.Reset();
            window.ViewModel.ArchiveInfo = this.ArchiveInfo;
            window.ViewModel.DestinationFolder = destinationFolder;
            window.ViewModel.ExtractAll = true;
            window.ShowInTaskbar = true;
            window.Owner = Application.Current.MainWindow;

            window.ShowDialog();

            this.OnExtractionCompleted?.Invoke(this, new ExtractionEventArgs("", (int)archiveInfo.TotalFiles, destinationFolder,
                window.ViewModel.ExtractionState));
        }

        public void ExtractFilesWithDialog(IList<int> files)
        {
            if (files == null)
                throw new ArgumentNullException(nameof(files));


                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                dialog.Description = "Extract files to folder...";
                dialog.ShowNewFolderButton = true;

                if (!String.IsNullOrWhiteSpace(AppSettings.Instance.Global.ExtractionLatestFolder))
                    dialog.SelectedPath = AppSettings.Instance.Global.ExtractionLatestFolder;

                var result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    ExtractFiles(dialog.SelectedPath, files);
                }
                else
                {
                    return;
                }

        }

        public void RemoveRecentArchive(string filePath)
        {
            AppSettings.Instance.MainWindow.RecentArchives.Remove(filePath);
            this.RecentArchives.Remove(filePath);
        }

        #region Private methods

        /// <summary>
        /// Extracts the files with dialog.
        /// </summary>
        /// <param name="destinationFolder">The destination folder.</param>
        /// <param name="files">The files.</param>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        private void ExtractFiles(string destinationFolder, IList<int> files)
        {
            if (String.IsNullOrWhiteSpace(destinationFolder))
                throw new ArgumentException(nameof(destinationFolder));
            if (files == null)
                throw new ArgumentNullException(nameof(files));

            AppSettings.Instance.Global.ExtractionLatestFolder = Path.GetDirectoryName(destinationFolder);

            FileExtractionWindow window = new FileExtractionWindow();
            window.ViewModel.Reset();
            window.ViewModel.ArchiveInfo = this.ArchiveInfo;
            window.ViewModel.DestinationFolder = destinationFolder;
            window.ViewModel.FilesToExtract = files;
            window.ShowInTaskbar = true;
            window.Owner = Application.Current.MainWindow;

            window.ShowDialog();

            this.OnExtractionCompleted?.Invoke(this, new ExtractionEventArgs("", files.Count, destinationFolder, 
                window.ViewModel.ExtractionState));
        }

        public bool AssociateExtension(Window parentWindow)
        {
            if (ExtensionAssociation.IsExtensionAssociated)
                return ExtensionAssociation.TryUnassociate(parentWindow, alreadyTriedToUnassociate: false);
            else
                return ExtensionAssociation.TryAssociate(parentWindow, alreadyTriedToAssociate: false);
        }

        private string LowerFileNameExtension(string fileName)
        {
            int index = fileName.LastIndexOf('.');
            if (index == -1 || index == fileName.Length)
                return fileName;

            StringBuilder builder = new StringBuilder(fileName, 0, fileName.Length, fileName.Length);
            for (int i = index + 1; i < fileName.Length; i++)
            {
                builder[i] = Char.ToLower(builder[i]);
            }

            return builder.ToString();
        }

        #endregion
    }
}