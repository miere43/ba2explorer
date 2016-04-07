using Ba2Tools;
using GalaSoft.MvvmLight;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Ba2Explorer.ViewModel
{
    /// <summary>
    /// Wrapper over Ba2Tools.BA2Archive
    /// </summary>
    /// <seealso cref="GalaSoft.MvvmLight.ObservableObject" />
    public sealed class ArchiveInfo : ObservableObject, IDisposable
    {
        private ObservableCollection<string> files;

        /// <summary>
        /// Gets filenames in archive.
        /// </summary>
        public ObservableCollection<string> Files
        {
            get { return files; }
            private set
            {
                files = value;
                RaisePropertyChanged();
            }
        }

        private bool isOpened = false;

        /// <summary>
        /// Gets a value indicating whether archive is opened.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is opened; otherwise, <c>false</c>.
        /// </value>
        public bool IsOpened
        {
            get { return isOpened; }
            private set
            {
                isOpened = true;
                RaisePropertyChanged();
            }
        }

        private bool isBusy = false;

        /// <summary>
        /// Gets a value indicating whether this instance is busy.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is busy; otherwise, <c>false</c>.
        /// </value>
        public bool IsBusy
        {
            get { return isBusy; }
            private set
            {
                isBusy = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Opened archive file name.
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// Opened archive file path.
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// Total files in archive.
        /// </summary>
        public int TotalFiles
        {
            get { return (int)archive.TotalFiles; }
        }

        private BA2Archive archive;

        void ThrowIfBusy()
        {
            if (IsBusy)
                throw new InvalidOperationException("I'am busy!");
        }

        public ArchiveInfo()
        {

        }

        public void ExtractToStream(Stream stream, string fileName)
        {
            ThrowIfBusy();

            try
            {
                IsBusy = true;
                archive.ExtractToStream(fileName, stream);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void ExtractFile(string fileName)
        {
            ThrowIfBusy();

            try
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.CheckPathExists = true;
                dialog.OverwritePrompt = true;
                dialog.ValidateNames = true;
                dialog.Title = "Extract file as...";
                dialog.FileName = Path.GetFileName(fileName);

                string ext = Path.GetExtension(dialog.SafeFileName);

                dialog.Filter = "Specified file|*" + ext + "|All files|*.*";
                dialog.ShowDialog();

                using (FileStream stream = File.Create(dialog.FileName))
                {
                    IsBusy = true;
                    archive.ExtractToStream(fileName, stream);
                }

                // MessageBox.Show("OK!");
            }
            catch (Exception e)
            {
                // todo;
                MessageBox.Show(e.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void ExtractFiles(
            IEnumerable<string> files,
            string destination,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            ThrowIfBusy();

            try
            {
                IsBusy = true;
                archive.ExtractFiles(files, destination, cancellationToken, progress, true);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void ExtractFiles(IEnumerable<string> files)
        {
            ThrowIfBusy();

            try
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                dialog.Description = "Save files to folder";
                dialog.ShowNewFolderButton = true;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    IsBusy = true;
                    archive.ExtractFiles(files, dialog.SelectedPath, true);
                }
            }
            catch (Exception e)
            {
                // todo;
                MessageBox.Show(e.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public static ArchiveInfo Open(string path)
        {
            ArchiveInfo info = null;

            //if (archive != null && IsOpened)
            //{
            //    if (IsBusy)
            //        throw new ArgumentException();

            //    archive.Dispose();
            //    archive = null;
            //    IsOpened = false;
            //    Files = null;
            //    FilePath = null;
            //    FileName = null;
            //}

            try
            {
                BA2Archive archive = BA2Loader.Load(path);

                info = new ArchiveInfo();
                info.archive = archive;
                info.Files = new ObservableCollection<string>(archive.ListFiles());
                info.FilePath = path;
                info.FileName = Path.GetFileName(info.FilePath);
            }
            catch (BA2LoadException e)
            {
                // todo
                MessageBox.Show($"Error while loading archive: {e.Message}");
            }
            catch (Exception e)
            {
                MessageBox.Show($"Unexcepted error while opening archive: {e.Message}");
            }

            return info;
        }

        public void Dispose()
        {
            ThrowIfBusy();

            if (archive != null)
                archive.Dispose();
        }
    }
}
