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

namespace Ba2Explorer.ViewModel
{
    /// <summary>
    /// Wrapper over Ba2Tools.BA2Archive
    /// </summary>
    /// <seealso cref="GalaSoft.MvvmLight.ObservableObject" />
    public sealed class ArchiveInfo : ObservableObject, IDisposable
    {
        private BA2Archive archive;

        private SemaphoreSlim accessSemaphore;

        #region Properties

        private ObservableCollection<string> files;

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
            accessSemaphore = new SemaphoreSlim(1, 1);
        }

        ~ArchiveInfo()
        {
            // TODO:
            // implement proper dispose.
            Dispose();
        }

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
        public Task<bool> ExtractToStreamAsync(Stream stream, string fileName, TimeSpan timeout, CancellationToken cancellationToken)
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
                    accessSemaphore.Wait(timeout, cancellationToken);
                    return archive.ExtractToStream(index, stream);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    App.Logger.Log(LogPriority.Error, "ArchiveInfo.ExtractToStreamAsync exception: {0}", e.Message);
                    throw;
                }
                finally
                {
                    accessSemaphore.Release();
                }
            });
        }

        /// <summary>
        /// Extracts file from archive to file in filesystem, aborting the process after certain timeout or when cancellation
        /// token signal received.
        /// </summary>
        /// <param name="stream">Destination stream where file would be extracted.</param>
        /// <param name="fileName">File name in archive to extract into stream.</param>
        /// <param name="timeout">Maximum time to wait until aborting extraction.
        /// Use Timeout.InfiniteTimeSpan to wait indefinitely.</param>
        /// <param name="cancellationToken">The cancellation token to observe.</param>
        /// <returns>False when <c>fileName</c> was not found in archive or error during extraction happened, or true otherwise.</returns>
        public Task<bool> ExtractFileAsync(string fileName, string destFileName, TimeSpan timeout, CancellationToken cancellationToken)
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
                    accessSemaphore.Wait(timeout, cancellationToken);
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
                finally
                {
                    accessSemaphore.Release();
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
                    accessSemaphore.Wait(timeout, cancellationToken);
                    archive.ExtractFiles(indexes, destFolder, cancellationToken, progress, true);
                    return true;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    App.Logger.LogException(LogPriority.Error, "ArchiveInfo.ExtractFilesAsync", e);
                    throw;
                }
                finally
                {
                    accessSemaphore.Release();
                }
            });
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

        void ThrowIfDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException("ArchiveInfo is disposed.");
        }

        public void Dispose()
        {
            if (archive != null)
                archive.Dispose();
            if (accessSemaphore != null)
                accessSemaphore.Dispose();

            IsDisposed = true;
        }
    }
}
