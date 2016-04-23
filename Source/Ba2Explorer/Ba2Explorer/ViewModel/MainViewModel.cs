using GalaSoft.MvvmLight;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.IO;
using System;
using System.Diagnostics.Contracts;
using Microsoft.Win32;
using Ba2Explorer.View;
using Ba2Explorer.Logging;
using Ba2Explorer.Settings;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;

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

        public MainWindow Window { get; set; }

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

        //private bool CharCmp(char c)
        //{
        //    return c == '\\';
        //}

        //public ObservableCollection<ArchiveFilename> CreateFromStrings(List<string> strings, int rootLevel = 0)
        //{
        //    strings.Sort((lhs, rhs) =>
        //    {
        //        int lhsDirs = lhs.Count(CharCmp);
        //        int rhsDirs = rhs.Count(CharCmp);

        //        if (lhsDirs > rhsDirs)
        //            return rhsDirs - lhsDirs;
        //        else if (rhsDirs < lhsDirs)
        //            return lhsDirs - rhsDirs;
        //        return 0;
        //    });

        //    ObservableCollection<ArchiveFilename> names = new ObservableCollection<ArchiveFilename>();

        //    foreach (string item in strings)
        //    {
        //        string[] subdirs = item.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
        //        if (subdirs.Length == 1)
        //        {
        //            names.Add(new ArchiveFilename(subdirs[0]));
        //            continue;
        //        }
        //    }

        //    return names;
        //}

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            ArchiveInfo = null;

            //ArchiveList = new ObservableCollection<ArchiveFilename>(
            //    CreateFromStrings(new List<string>()
            //    {
            //        @"1",
            //        @"2\b",
            //        @"3\b\c",
            //    }));

#if DEBUG
            if (IsInDesignMode)
                PrepareDesignTimeData();
#endif

            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}

            RecentArchives = AppSettings.Instance.MainWindow.GetLatestFiles();
            RecentArchives.CollectionChanged += (sender, args) => RaisePropertyChanged(nameof(HasRecentArchives));
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

        public void SetTitle(string title)
        {
            if (title == null)
                Window.Title = "BA2 Explorer";
            else
                Window.Title = "BA2 Explorer • " + title.Trim();
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
            AppSettings.Instance.MainWindow.PushRecentArchive(dialog.FileName, this.RecentArchives);
        }

        /// <summary>
        /// Opens the archive from file path.
        /// </summary>
        /// <exception cref="ArgumentException" />
        public void OpenArchive(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
                throw new ArgumentException(nameof(path));

            // Requested same archive.
            if (archiveInfo != null)
                if (path.Equals(this.archiveInfo.FilePath, StringComparison.OrdinalIgnoreCase))
                    return;

            if (ArchiveInfo != null)
            {
                CloseArchive(false);
            }

            // TODO:
            // check for errors
            ArchiveInfo = ArchiveInfo.Open(path);
            SetTitle(ArchiveInfo.FileName);

            AppSettings.Instance.Global.OpenArchiveLatestFolder = Path.GetDirectoryName(path);
            App.Logger.Log(LogPriority.Info, "Opened archive {0}", path);
            Window.ShowStatusBar(path);
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
            if (resetUI)
            {
                SetTitle(null);
                Window.CollapseStatusBar();
            }
        }

        /// <summary>
        /// Extracts file from archive to some file in user file system, asking him where to save file using SaveFileDialog.
        /// </summary>
        /// <param name="fileName">File name in archive.</param>
        /// <returns>Task.</returns>
        public async Task ExtractFileWithDialog(string fileName)
        {
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
                await ArchiveInfo.ExtractFileAsync(fileName, dialog.FileName, Timeout.InfiniteTimeSpan, CancellationToken.None);
                AppSettings.Instance.Global.ExtractionLatestFolder = Path.GetDirectoryName(dialog.FileName);
                Window.ShowStatusBarWithButton($"File \"{fileName}\" extracted.", "Open file folder",
                    () => ExplorerOpenPath(dialog.FileName, true));
            }
        }

        public void ExtractFilesWithDialog(IEnumerable<string> files)
        {
            if (files == null)
                throw new ArgumentNullException(nameof(files));

            try
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                dialog.Description = "Extract files to folder...";
                dialog.ShowNewFolderButton = true;

                if (!String.IsNullOrWhiteSpace(AppSettings.Instance.Global.ExtractionLatestFolder))
                    dialog.SelectedPath = AppSettings.Instance.Global.ExtractionLatestFolder;

                var result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    ExtractFiles(dialog.SelectedPath, files);
                    AppSettings.Instance.Global.ExtractionLatestFolder = dialog.SelectedPath;
                }
                else
                {
                    return;
                }
            }
            catch (Exception e)
            {
                App.Logger.Log(LogPriority.Error, "ExtractFilesWithDialog failed: {0}", e.Message);
                // todo;
                MessageBox.Show(e.Message);
            }
        }

        #region Private methods

        /// <summary>
        /// Opens Explorer, which opens directory or selecting the file passed in <c>path</c> argument.
        /// </summary>
        private void ExplorerOpenPath(string path, bool isFile)
        {
            ProcessStartInfo info = new ProcessStartInfo();
            info.ErrorDialog = true;
            info.ErrorDialogParentHandle = new WindowInteropHelper(this.Window).Handle;
            info.FileName = "explorer";
            info.UseShellExecute = true;

            bool failed = false;

            if (isFile)
            {
                if (File.Exists(path)) {
                    info.Arguments = @"/select," + path;
                } else if (Directory.Exists(Path.GetDirectoryName(path))) {
                    info.Arguments = Path.GetDirectoryName(path);
                } else {
                    failed = true;
                }
            }
            else
            {
                if (Directory.Exists(path)) {
                    info.Arguments = path;
                } else {
                    failed = true;
                }
            }

            if (failed)
            {
                MessageBox.Show(this.Window, "Cannot open file folder: file nor folder are exist", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Warning, MessageBoxResult.OK, MessageBoxOptions.None);
                return;
            }

            var process = Process.Start(info);
            process.Dispose();
        }

        /// <summary>
        /// Extracts the files with dialog.
        /// </summary>
        /// <param name="destinationFolder">The destination folder.</param>
        /// <param name="files">The files.</param>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        private void ExtractFiles(string destinationFolder, IEnumerable<string> files)
        {
            if (String.IsNullOrWhiteSpace(destinationFolder))
                throw new ArgumentException(nameof(destinationFolder));
            if (files == null)
                throw new ArgumentNullException(nameof(files));

            AppSettings.Instance.Global.ExtractionLatestFolder = Path.GetDirectoryName(destinationFolder);

            FileExtractionWindow window = new FileExtractionWindow();
            window.ViewModel.ArchiveInfo = this.ArchiveInfo;
            window.ViewModel.DestinationFolder = destinationFolder;
            window.ViewModel.FilesToExtract = files;
            window.ShowInTaskbar = true;
            window.Owner = this.Window;

            window.ShowDialog();

            if (window.ViewModel.IsFinishedSuccessfully)
            {
                Window.ShowStatusBarWithButton($"{ window.ViewModel.FilesToExtract.Count() } files were extracted.", "Open folder",
                    () => ExplorerOpenPath(window.ViewModel.DestinationFolder, false));
            }
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