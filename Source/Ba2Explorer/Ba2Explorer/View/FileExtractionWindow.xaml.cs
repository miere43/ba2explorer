using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.ComponentModel;
using Ba2Explorer.ViewModel;
using Ba2Explorer.Utility;

namespace Ba2Explorer.View
{
    /// <summary>
    /// Логика взаимодействия для FileExtractionWindow.xaml
    /// </summary>
    public partial class FileExtractionWindow : Window
    {
        private bool started = false;

        public FileExtractionViewModel ViewModel;

        public FileExtractionWindow()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            ViewModel = (FileExtractionViewModel)DataContext;

            this.Loaded += FileExtractionWindow_Loaded;

            base.OnInitialized(e);
        }

        private void FileExtractionWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.OnFinished += ViewModel_OnFinished;
            ViewModel.ExtractionProgress.ProgressChanged += ExtractionProgress_ProgressChanged;

            this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
            this.ExtractionProgress.IsIndeterminate = true;

            this.Title = "Extracting " + ViewModel.ArchiveInfo.FileName;

            var task = ViewModel.ExtractFiles();
        }

        private void ViewModel_OnFinished(object sender, ExtractionFinishedState e)
        {
            this.Dispatcher.Invoke(() =>
            {
                ViewModel.OnFinished -= ViewModel_OnFinished;
                ViewModel.ExtractionProgress.ProgressChanged -= ExtractionProgress_ProgressChanged;

                UpdateExtractionProgress(ViewModel.ExtractionFileCount, ViewModel.ExtractionFileCount, true);
                SetExtractingWindowTitle(ExtractionProgress.Value);

                if (e == ExtractionFinishedState.Canceled)
                {
                    this.Cancel.Content = "Canceled";
                    this.Title = "Canceled " + ViewModel.ArchiveInfo.FileName;
                }
                else
                {
                    if (NotifyOnFinishedCheckBox.IsChecked.Value)
                    {
                        MessageBox.Show(this, "Extraction completed.", "Extraction completed.", MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }

                this.Close();
            });
        }

        private void SetExtractingWindowTitle(double percent)
        {
            this.Title = String.Format("{0:P0} - {1}", percent, ViewModel.ArchiveInfo.FileName);
        }

        private void UpdateExtractionProgress(int actual, int excepted, bool final)
        {
            if (!started)
            {
                started = true;
                this.ExtractionProgress.IsIndeterminate = false;
                this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
            }

            if (final)
            {
                this.MainText.Text = $"Extracted { actual } out of { excepted } files.";
                ExtractionProgress.Value = 1.0d;
                this.TaskbarItemInfo.ProgressValue = 1.0d;
            }
            else
            {
                this.MainText.Text = $"Extracted { actual } out of { excepted } files…";
                ExtractionProgress.Value = (double)actual / ViewModel.ExtractionFileCount;
                this.TaskbarItemInfo.ProgressValue = ExtractionProgress.Value;
            }

            SetExtractingWindowTitle(ExtractionProgress.Value);
        }

        private void ExtractionProgress_ProgressChanged(object sender, int e)
        {
            UpdateExtractionProgress(e, ViewModel.ExtractionFileCount, false);
        }

        private void CanStopExtraction(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ViewModel?.ArchiveInfo == null)
                return;

            e.CanExecute = ViewModel.IsExtracting;
            e.Handled = true;
        }

        private void CanOpenFolder(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void OpenFolderExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            OpenAppUtil.ExplorerOpenPath(this, ViewModel.DestinationFolder, false);
        }

        private void StopExtraction(object sender, ExecutedRoutedEventArgs e)
        {
            Cancel.Content = "Canceling...";
            Cancel.IsEnabled = false;
            this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Paused;

            ViewModel.Cancel();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // TODO hide close button on window.
            if (ViewModel.IsExtracting)
                e.Cancel = true;
            else
                e.Cancel = false;
        }
    }
}
