using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using Ba2Tools;
using Ba2Explorer.Logging;
using Ba2Explorer.Settings;
using System.Linq;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace Ba2Explorer.ViewModel
{
    /// <summary>
    /// Wrapper over Ba2Tools.BA2Archive
    /// </summary>
    /// <seealso cref="GalaSoft.MvvmLight.ObservableObject" />
    public sealed class ArchiveInfo : INotifyPropertyChanged, IDisposable
    {
        private object m_lock = new object();

        private BA2Archive m_archive;
        public BA2Archive Archive
        {
            get
            {
                return m_archive;
            }
            set
            {
                    m_archive = value;
                    RaisePropertyChanged();
            }
        }

        public event EventHandler Disposing;
        public event PropertyChangedEventHandler PropertyChanged;

        #region Properties

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

        private ObservableCollection<string> files;

        /// <summary>
        /// Gets filenames in archive.
        /// </summary>
        public ObservableCollection<string> FileNames
        {
            get { return files; }
            private set
            {
                files = value;
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
            get { return (int)Archive.TotalFiles; }
        }

        #endregion

        public ArchiveInfo()
        {
        }

        ~ArchiveInfo()
        {
            // TODO:
            // implement proper dispose.
            Dispose();
        }

        #region Public methods

        public BA2Archive GetArchive() { return Archive; }

        public Task ExtractAllAsync(string destFolder, CancellationToken cancellationToken, IProgress<int> progress)
        {
            return Task.Run(() =>
            {
                try
                {
                    Archive.ExtractAll(destFolder, true, cancellationToken, progress);
                }
                catch (BA2ExtractionException e)
                {
                    App.Logger.LogException(LogPriority.Error, "ArchiveInfo.ExtractAllAsync", e);
                    throw;
                }
            });
        }

        /// <summary>
        /// Extracts file from archive to file in filesystem, aborting the process after certain timeout or when cancellation
        /// token signal received.
        /// </summary>
        /// <param name="stream">Destination stream where file would be extracted.</param>
        /// <param name="fileName">File name in archive to extract into stream.</param>
        /// <returns>False when <c>fileName</c> was not found in archive or error during extraction happened, or true otherwise.</returns>
        public Task<bool> ExtractFileAsync(int fileIndex, string destFileName)
        {
            if (destFileName == null)
                throw new ArgumentNullException(nameof(destFileName));

            return Task.Run(() =>
            {
                try
                {
                    using (FileStream stream = File.Create(destFileName))
                    {
                        return Archive.ExtractToStream(fileIndex, stream);
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    App.Logger.Log(LogPriority.Error, "ArchiveInfo.ExtractToFileAsync exception: {0}", e.Message);
                    throw;
                }
            });
        }

        public Task<bool> ExtractFilesAsync(IEnumerable<int> fileIndexes, string destFolder, IProgress<int> progress, TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            if (files == null)
                throw new ArgumentNullException(nameof(files));
            if (destFolder == null)
                throw new ArgumentNullException(nameof(destFolder));

            return Task.Run(() =>
            {
                try
                {
                    Archive.ExtractFiles(fileIndexes, destFolder, true, cancellationToken, progress);
                    return true;
                }
                catch (BA2ExtractionException e)
                {
                    App.Logger.LogException(LogPriority.Error, "ArchiveInfo.ExtractFilesAsync", e);
                    throw;
                }
            });
        }

        public static ArchiveInfo Open(string path)
        {
            BA2Archive archive = BA2Loader.Load(path,
                AppSettings.Instance.Global.MultithreadedExtraction ? BA2LoaderFlags.Multithreaded : BA2LoaderFlags.None);

            ArchiveInfo info = new ArchiveInfo();
            info.Archive = archive;
            info.FileNames = new ObservableCollection<string>(archive.FileList);
            info.FilePath = path;
            info.FileName = Path.GetFileName(info.FilePath);

            return info;
        }

        #endregion

        #region Disposal

        void ThrowIfDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(ArchiveInfo));
        }

        public void Dispose()
        {
            if (IsDisposed) return;
            Disposing?.Invoke(this, null);

            if (Archive != null)
                Archive.Dispose();

            IsDisposed = true;
        }

        #endregion

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            Contract.Requires(propertyName != null);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
