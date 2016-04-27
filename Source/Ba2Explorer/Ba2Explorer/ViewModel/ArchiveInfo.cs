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
using Ba2Explorer.Logging;
using Ba2Explorer.Settings;

namespace Ba2Explorer.ViewModel
{
    /// <summary>
    /// Wrapper over Ba2Tools.BA2Archive
    /// </summary>
    /// <seealso cref="GalaSoft.MvvmLight.ObservableObject" />
    public sealed class ArchiveInfo : ObservableObject, IDisposable
    {
        private BA2Archive archive;

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
        }

        ~ArchiveInfo()
        {
            // TODO:
            // implement proper dispose.
            Dispose();
        }

        #region Public methods

        public bool Contains(string filePath)
        {
            return archive.ContainsFile(filePath);
        }

        /// <summary>
        /// Extracts file to stream, aborting the process after certain timeout or when
        /// cancellation token signal received.
        /// </summary>
        /// <param name="stream">Destination stream where file would be extracted.</param>
        /// <param name="fileName">File name in archive to extract into stream.</param>
        /// <param name="timeout">Maximum time to wait until aborting extraction.
        /// Use Timeout.InfiniteTimeSpan to wait indefinitely.</param>
        /// <param name="cancellationToken">The cancellation token to observe.</param>
        /// <returns>False when <c>fileName</c> was not found in archive or error during extraction happened, or true otherwise.</returns>
        public Task<bool> ExtractToStreamAsync(Stream stream, string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            return Task.Run(() => {
                var index = archive.GetIndexFromFilename(fileName);
                if (index == -1)
                    return false;

                try
                {
                    return archive.ExtractToStream(index, stream);
                }
                catch (BA2ExtractionException e)
                {
                    App.Logger.Log(LogPriority.Error, "ArchiveInfo.ExtractToStreamAsync exception: {0}", e.Message);
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
        public Task<bool> ExtractFileAsync(string fileName, string destFileName)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));
            if (destFileName == null)
                throw new ArgumentNullException(nameof(destFileName));

            return Task.Run(() =>
            {
                var index = archive.GetIndexFromFilename(fileName);
                if (index == -1)
                    return false;

                try
                {
                    using (FileStream stream = File.Create(destFileName))
                    {
                        return archive.ExtractToStream(index, stream);
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

        public Task<bool> ExtractFilesAsync(IEnumerable<string> files, string destFolder, IProgress<int> progress, TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            if (files == null)
                throw new ArgumentNullException(nameof(files));
            if (destFolder == null)
                throw new ArgumentNullException(nameof(destFolder));

            return Task.Run(() =>
            {
                List<int> indexes = new List<int>();
                foreach (var fileName in files)
                {
                    int index = archive.GetIndexFromFilename(fileName);
                    if (index == -1)
                        return false;

                    indexes.Add(index);
                }

                try
                {
                    archive.ExtractFiles(indexes, destFolder, true, cancellationToken, progress);
                    return true;
                }
                catch (BA2LoadException e)
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
            info.archive = archive;
            info.Files = new ObservableCollection<string>(archive.FileList);
            info.FilePath = path;
            info.FileName = Path.GetFileName(info.FilePath);

            return info;
        }

        #endregion

        #region Disposal

        void ThrowIfDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException("ArchiveInfo is disposed.");
        }

        public void Dispose()
        {
            if (archive != null)
                archive.Dispose();

            IsDisposed = true;
        }

        #endregion
    }
}
