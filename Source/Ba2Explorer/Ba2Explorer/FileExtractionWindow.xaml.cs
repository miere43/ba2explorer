﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Ba2Explorer.ViewModel;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Interop;

namespace Ba2Explorer
{
    /// <summary>
    /// Логика взаимодействия для FileExtractionWindow.xaml
    /// </summary>
    public partial class FileExtractionWindow : Window
    {
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

            this.Title = "Extracting " + ViewModel.ArchiveInfo.FileName;

            Debug.WriteLine("file extr activated");
            ViewModel.ExtractFiles();
        }

        private void ViewModel_OnFinished(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                SetExtractingText(ViewModel.FilesToExtract.Count(), ViewModel.FilesToExtract.Count());
                ExtractionProgress.Value = 1.0d;
                ViewModel.OnFinished -= ViewModel_OnFinished;
                ViewModel.ExtractionProgress.ProgressChanged -= ExtractionProgress_ProgressChanged;
                this.Title = "Finished extracting " + ViewModel.ArchiveInfo.FileName;
            });
        }

        private void SetExtractingText(int actual, int excepted)
        {
            this.MainText.Text = "Extracting " + actual + "/" + excepted;
        }

        private void ExtractionProgress_ProgressChanged(object sender, int e)
        {
            ExtractionProgress.Value = (double)e / ViewModel.FilesToExtract.Count();
            SetExtractingText(e, ViewModel.FilesToExtract.Count());
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
            Process.Start(ViewModel.DestinationFolder);
        }

        private void StopExtraction(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("stop extr");
            Cancel.Content = "Canceling...";
            Cancel.IsEnabled = false;

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
