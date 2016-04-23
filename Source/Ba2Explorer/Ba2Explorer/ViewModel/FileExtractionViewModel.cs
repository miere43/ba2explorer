using Ba2Tools;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Ba2Explorer.ViewModel
{
    public sealed class FileExtractionViewModel : ViewModelBase
    {
        private CancellationTokenSource cancellationToken;

        public event EventHandler OnFinishedSuccessfully;

        public event EventHandler OnCanceled;

        public event EventHandler OnExtractionError;

        public Progress<int> ExtractionProgress { get; set; }

        #region Properties

        private ArchiveInfo archiveInfo;
        public ArchiveInfo ArchiveInfo
        {
            get { return archiveInfo; }
            set
            {
                archiveInfo = value;
                RaisePropertyChanged();
            }
        }

        private bool isExtracting = false;
        public bool IsExtracting
        {
            get { return isExtracting; }
            private set
            {
                isExtracting = value;
                Debug.WriteLine("changed to " + isExtracting);
                RaisePropertyChanged();
            }
        }

        private bool isExtractionFinished = false;
        public bool IsExtractionFinished
        {
            get { return isExtractionFinished; }
            private set
            {
                isExtractionFinished = value;
                RaisePropertyChanged();
            }
        }

        private bool isExtractionCancelled = false;
        public bool IsExtractionCancelled
        {
            get { return isExtractionCancelled; }
            private set
            {
                isExtractionCancelled = value;
                RaisePropertyChanged();
            }
        }

        private bool isFinishedSuccessfully = false;
        public bool IsFinishedSuccessfully
        {
            get { return isFinishedSuccessfully; }
            private set
            {
                isFinishedSuccessfully = value;
                RaisePropertyChanged();
            }
        }

        private string destinationFolder = "";
        public string DestinationFolder
        {
            get { return destinationFolder; }
            set
            {
                destinationFolder = value;
                RaisePropertyChanged();
            }
        }

        private IEnumerable<string> filesToExtract = null;
        public IEnumerable<string> FilesToExtract
        {
            get { return filesToExtract; }
            set
            {
                filesToExtract = value;
                RaisePropertyChanged();
            }
        }

#endregion

        public FileExtractionViewModel()
        {
            ExtractionProgress = new Progress<int>();
            cancellationToken = new CancellationTokenSource();
        }

        public void Cancel()
        {
            Contract.Ensures(IsExtracting == true);

            cancellationToken.Cancel();
        }

        public async Task ExtractFiles()
        {
            try
            {
                IsExtracting = true;
                await ArchiveInfo.ExtractFilesAsync(FilesToExtract, DestinationFolder, ExtractionProgress, Timeout.InfiniteTimeSpan,
                    cancellationToken.Token);
                IsFinishedSuccessfully = true;
                OnFinishedSuccessfully?.Invoke(this, null);
            }
            catch (OperationCanceledException)
            {
                OnCanceled?.Invoke(this, null);
            }
            catch (BA2ExtractionException)
            {
                OnExtractionError?.Invoke(this, null);
            }
            finally
            {
                IsExtracting = false;
            }
        }
    }
}
