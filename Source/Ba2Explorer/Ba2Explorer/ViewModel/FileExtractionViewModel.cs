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
    public enum ExtractionFinishedState
    {
        None,
        Succeed,
        Failed,
        Canceled
    }

    public sealed class FileExtractionViewModel : ViewModelBase
    {
        private CancellationTokenSource cancellationToken;

        #region Properties / Events

        public event EventHandler<ExtractionFinishedState> OnFinished;

        public Progress<int> ExtractionProgress { get; set; }

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

        private ExtractionFinishedState extractionState = ExtractionFinishedState.None;
        public ExtractionFinishedState ExtractionState
        {
            get { return extractionState; }
            private set
            {
                extractionState = value;
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
            Reset();
        }

        public void Reset()
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

                ExtractionState = ExtractionFinishedState.Succeed;
            }
            catch (OperationCanceledException)
            {
                ExtractionState = ExtractionFinishedState.Canceled;
            }
            catch (BA2ExtractionException)
            {
                ExtractionState = ExtractionFinishedState.Failed;
            }
            finally
            {
                IsExtracting = false;
                OnFinished?.Invoke(this, ExtractionState);
            }
        }
    }
}
