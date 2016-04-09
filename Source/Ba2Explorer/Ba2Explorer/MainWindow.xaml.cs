using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Ba2Explorer.ViewModel;
using System.Collections;

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
                if (args.PropertyName == nameof(ArchiveInfo) && mainViewModel.ArchiveInfo != null)
                {
                    this.FilePreview.SetArchive(mainViewModel.ArchiveInfo);
                    archiveFilesFilter = (CollectionView)CollectionViewSource.GetDefaultView(this.ArchiveFilesList.ItemsSource);
                    archiveFilesFilter.Filter = ArchiveFileFilter;
                }
            };

            this.Loaded += MainWindow_Loaded;
        }


        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // mainViewModel.OpenArchive("D:/Games/Fallout 4/Data/DLCRobot - Textures.ba2");
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

        private void ExtractSingleCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            string sel = ArchiveFilesList.SelectedItem as string;

            mainViewModel.ArchiveInfo.ExtractFileWithDialog(sel);
            e.Handled = true;
        }

        private void ExtractAllCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            IList sels = ArchiveFilesList.SelectedItems;

            mainViewModel.ExtractFilesWithDialog(sels.OfType<string>().ToList());
            e.Handled = true;
        }

        #endregion
    }
}
