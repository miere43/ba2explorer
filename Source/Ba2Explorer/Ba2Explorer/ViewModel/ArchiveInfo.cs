using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using GalaSoft.MvvmLight;
using Ba2Tools;

namespace Ba2Explorer.ViewModel
{
    /// <summary>
    /// Wrapper over Ba2Tools.BA2Archive
    /// </summary>
    /// <seealso cref="GalaSoft.MvvmLight.ObservableObject" />
    public sealed class ArchiveInfo : ObservableObject, IDisposable
    {
        private BA2Archive archive;

        private object extractionLock = new object();

        private BlockingCollection<object> extractionStack;

        #region Properties

        private ObservableCollection<string> files;

        private bool isBusy = false;

        private bool isDisposed = false;

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return isDisposed; }
            private set
            {
                isDisposed = value;
                RaisePropertyChanged();
            }
        }

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

        /// <summary>
        /// Gets a value indicating whether this instance is busy.
        /// </summary>
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

        #endregion

        public ArchiveInfo()
        {
            extractionStack = new BlockingCollection<object>();
        }

        ~ArchiveInfo()
        {
            Dispose();
        }

        public bool Contains(string filePath)
        {
            return archive.ContainsFile(filePath);
        }

        public void QueueExtractFile(string filePath, CancellationToken token)
        {
            extractionStack.Add(filePath);
        }

        //public void ServeAsync()
        //{
        //    while (extractionStack.)
        //}

        /// <summary>
        /// Tries to extract file to stream, if archive is busy extracting another file,
        /// it will wait all queued extractions finishes.
        /// </summary>
        public async Task ExtractToStreamAsync(string filePath, Stream stream)
        {
            ThrowIfDisposed();

            await Task.Run(() =>
            {
                lock (extractionLock)
                {
                    archive.ExtractToStream(filePath, stream);
                }
            });
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

        public void ExtractFileWithDialog(string fileName)
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

        public void ExtractFilesWithDialog(IEnumerable<string> files)
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

        void ThrowIfBusy()
        {
            if (IsBusy)
                throw new InvalidOperationException("Archive is already busy extracting files.");
        }

        void ThrowIfDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException("ArchiveInfo is disposed.");
        }

        public void Dispose()
        {
            ThrowIfBusy();

            if (extractionStack != null)
                extractionStack.Dispose();
            if (archive != null)
                archive.Dispose();

            IsDisposed = true;
        }
    }
}
