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
    public class FileExtractionViewModel : ViewModelBase
    {
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
            set
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
            set
            {
                isExtractionFinished = value;
                RaisePropertyChanged();
            }
        }

        private bool isExtractionCancelled = false;
        public bool IsExtractionCancelled
        {
            get { return isExtractionCancelled; }
            set
            {
                isExtractionCancelled = value;
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

        public Progress<int> ExtractionProgress { get; set; }

        private CancellationTokenSource cancellationToken;

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

        public event EventHandler OnFinishedSuccessfully;

        public event EventHandler OnCanceled;

        public event EventHandler OnExtractionError;

        public void ExtractFiles()
        {
            Contract.Ensures(IsExtracting == false);

            Debug.WriteLine("begin extr");
            try
            {
                var task = Task.Run(() => 
                {
                    try
                    {
                        IsExtracting = true;
                        ArchiveInfo.ExtractFiles(FilesToExtract, DestinationFolder, cancellationToken.Token, ExtractionProgress);
                        if (OnFinishedSuccessfully != null)
                            OnFinishedSuccessfully(this, null);
                    }
                    catch (OperationCanceledException)
                    {
                        if (OnCanceled != null)
                            OnCanceled(this, null);
                    }
                    catch (BA2ExtractionException)
                    {
                        if (OnExtractionError != null)
                            OnExtractionError(this, null);
                    }
                    finally
                    {
                        IsExtracting = false;
                    }
                });
            } 
            catch (OperationCanceledException)
            {
                // todo;
                MessageBox.Show("cancelled.");
                IsExtracting = false;
            }
            finally
            {
                IsExtracting = false;
            }
        }
    }
}
