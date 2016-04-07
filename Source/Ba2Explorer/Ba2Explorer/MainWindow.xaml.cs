using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ba2Explorer.ViewModel;
using Microsoft.Win32;
using System.Collections;
using System.Diagnostics;
using System.IO;

namespace Ba2Explorer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel mainViewModel;

        public MainWindow()
        {
            InitializeComponent();

            mainViewModel = (MainViewModel)DataContext;
            mainViewModel.Window = this;

            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            mainViewModel.OpenArchive("D:/Games/Fallout 4/Data/DLCRobot - Main.ba2");
            //mainViewModel.ExtractFiles("D:/A", mainViewModel.ArchiveInfo.Files);
        }

        private void OpenCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            mainViewModel.OpenArchiveWithDialog();
            e.Handled = true;
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

        private void ArchiveFilesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO handle multiple selection
            if (this.ArchiveFilesList.SelectedItem == null)
                return;

            if (mainViewModel.ArchiveInfo.IsBusy)
                return;

            string selectedFilePath = (string)this.ArchiveFilesList.SelectedItem;

            if (this.FilePreview.CanPreviewTarget(selectedFilePath))
            {
                using (MemoryStream stream = new MemoryStream()) {
                    mainViewModel.ArchiveInfo.ExtractToStream(stream, selectedFilePath);

                    this.FilePreview.SetPreviewTarget(stream, selectedFilePath);
                }

            }
            else
            {
                this.FilePreview.SetUnknownPreviewTarget(selectedFilePath);
            }

            e.Handled = true;
        }
    }
}
