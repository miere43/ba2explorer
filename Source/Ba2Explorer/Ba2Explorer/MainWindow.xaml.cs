using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Ba2Explorer.ViewModel;
using System.Collections;
using Nett;
using Ba2Explorer.Settings;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using Ba2Explorer.Utility;

namespace Ba2Explorer
{
    public partial class MainWindow : Window
    {
        private MainViewModel mainViewModel;

        private CollectionView archiveFilesFilter;

        public MainWindow()
        {
            InitializeComponent();

            mainViewModel = (MainViewModel)DataContext;
            mainViewModel.Window = this;

            mainViewModel.PropertyChanged += (sender, args) =>
            {
                if (mainViewModel.ArchiveInfo != null && args.PropertyName == nameof(ArchiveInfo))
                {
                    // todo string.intern(nameof(archiveinfo)) ?
                    this.FilePreview.SetArchive(mainViewModel.ArchiveInfo);
                    archiveFilesFilter = (CollectionView)CollectionViewSource.GetDefaultView(this.ArchiveFilesList.ItemsSource);
                    archiveFilesFilter.Filter = ArchiveFileFilter;
                }
            };

            var settings = AppSettings.Instance.MainWindow;
            this.Width = settings.WindowWidth;
            this.Height = settings.WindowHeight;
            this.Topmost = settings.Topmost;

            if (!AppSettings.Instance.Global.IsFirstLaunch)
            {
                this.Top = settings.WindowTop;
                this.Left = settings.WindowLeft;
            }

            settings.OnSaving += Settings_OnSaving;

            this.Loaded += MainWindow_Loaded;
            this.Closed += MainWindow_Closed;

            // OpenSettingsExecuted(null, null);
        }

        private void Settings_OnSaving(object sender, EventArgs e)
        {
            MainWindowSettings s = (MainWindowSettings)sender;
            s.Topmost = this.Topmost;
            s.WindowLeft = this.Left;
            s.WindowTop = this.Top;
            s.WindowWidth = this.Width;
            s.WindowHeight = this.Height;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            this.mainViewModel.Cleanup();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            // mainViewModel.OpenArchive("D:/Games/Fallout 4/Data/Fallout4 - Sounds.ba2");
            //mainViewModel.ExtractFiles("D:/A", mainViewModel.ArchiveInfo.Files);
        }

        private void ArchiveFilesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO handle multiple selection
            if (this.ArchiveFilesList.SelectedItem == null)
                return;

            if (mainViewModel.ArchiveInfo.IsBusy)
                return;

            string selectedFilePath = (string)this.ArchiveFilesList.SelectedItem;

            Task<bool> task = this.FilePreview.TrySetPreviewAsync(selectedFilePath);

            e.Handled = true;
        }

        #region Archive files filter

        private bool ArchiveFileFilter(object item)
        {
            if (String.IsNullOrWhiteSpace(this.FilterText.Text))
                return true;
            else
            {
                string filePath = (string)item;
                return filePath.IndexOf(this.FilterText.Text, StringComparison.OrdinalIgnoreCase) >= 0;
            }
        }

        private void FilterText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (mainViewModel.ArchiveInfo == null)
                return;

            CollectionViewSource.GetDefaultView(this.ArchiveFilesList.ItemsSource).Refresh();
        }

        #endregion

        #region Commands

        private void OpenCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            mainViewModel.OpenArchiveWithDialog();
            e.Handled = true;
        }

        private void CloseCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mainViewModel.ArchiveInfo != null;
            e.Handled = true;
        }

        private void CloseCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            mainViewModel.CloseArchive();

            GC.Collect(2);
        }

        private void ExtractCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (mainViewModel.ArchiveInfo == null)
            {
                e.CanExecute = false;
                e.Handled = true;
                return;
            }

            e.CanExecute = ArchiveFilesList.SelectedIndex != -1;
            e.Handled = true;
        }

        private void ExtractSelectedExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (ArchiveFilesList.SelectedItems.Count == 1)
            {
                string sel = ArchiveFilesList.SelectedItem as string;

                mainViewModel.ExtractFileWithDialog(sel);
                e.Handled = true;
            }
            else if (ArchiveFilesList.SelectedItems.Count > 1)
            {
                IList sels = ArchiveFilesList.SelectedItems;

                mainViewModel.ExtractFilesWithDialog(sels.Cast<string>());
                e.Handled = true;
            }
        }

        private void ExtractAllCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            mainViewModel.ExtractFilesWithDialog(mainViewModel.ArchiveInfo.Files);
            e.Handled = true;
        }

        private void ExtractAllCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mainViewModel.ArchiveInfo != null && !mainViewModel.ArchiveInfo.IsBusy;
            e.Handled = true;
        }

        private void OpenSettingsExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            OpenAppUtil.RunFileSafe("prefs.toml");
        }

        private void ExitAppExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void CollectGCExecuted(object sender, RoutedEventArgs e)
        {
            long memoryBefore = GC.GetTotalMemory(false);
            GC.Collect(GC.MaxGeneration);
            long memoryAfter = GC.GetTotalMemory(false);

            MessageBox.Show(String.Format("Collected {0} generations.\n\nBefore: {1:n2} MB\nAfter: {2:n2} MB",
                GC.MaxGeneration,
                memoryBefore / 1024.0d / 1024.0d,
                memoryAfter  / 1024.0d / 1024.0d));
        }

        #endregion

        private void RecentArchivesItemExecuted(object sender, RoutedEventArgs e)
        {
            MenuItem obj = sender as MenuItem;

            if (obj.Items.CurrentPosition == -1)
                return;

            string item = obj.Items.CurrentItem as string;
            if (item == null)
                return;

            mainViewModel.OpenArchive(item);
        }
    }
}
