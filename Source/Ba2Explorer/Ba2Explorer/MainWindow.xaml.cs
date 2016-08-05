using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Ba2Explorer.ViewModel;
using Ba2Explorer.Settings;
using Ba2Explorer.Utility;
using System.Collections.Generic;
using Ba2Explorer.Service;
using System.Diagnostics;

namespace Ba2Explorer
{
    public partial class MainWindow : Window
    {
        private MainViewModel viewModel;

        private CollectionView archiveFilesFilter;

        private Action statusbarButtonAction;

        public MainWindow()
        {
            InitializeComponent();

            viewModel = (MainViewModel)DataContext;
            viewModel.OnArchiveOpened += ViewModel_OnArchiveOpened;
            viewModel.OnArchiveClosed += ViewModel_OnArchiveClosed;
            viewModel.OnExtractionCompleted += ViewModel_OnExtractionCompleted;

            this.Loaded += MainWindow_Loaded;
            this.Closed += MainWindow_Closed;

            this.FilePreview.IsEnabledChanged += (sender, args) =>
            {
                // if (FilePreview.IsEnabled) SetSelectedItemFilePreview();
            };

            // Settings handling
            AppSettings.Instance.MainWindow.OnSaving  += MainWindowSettings_OnSaving;
            AppSettings.Instance.FilePreview.OnSaving += FilePreviewSettings_OnSaving;

            MainWindowSettings settings = AppSettings.Instance.MainWindow;
            this.Width   = settings.WindowWidth;
            this.Height  = settings.WindowHeight;
            this.Topmost = settings.Topmost;

            if (!AppSettings.Instance.Global.IsFirstLaunch)
            {
                this.Top = settings.WindowTop;
                this.Left = settings.WindowLeft;
            }

            //DependencyPropertyDescriptor.FromProperty(ListView.SelectedItemProperty, typeof(ListView))
            //    .AddValueChanged(this, (s, e) =>
            //    {
            //        var i = 123;
            //    });

            // TODO: fix this
            this.FilePreviewPanelMenuItem.IsChecked = AppSettings.Instance.FilePreview.IsEnabled;
        }

        #region Settings Saving / Loading

        private void MainWindowSettings_OnSaving(object sender, EventArgs e)
        {
            MainWindowSettings s = (MainWindowSettings)sender;
            s.Topmost = this.Topmost;
            s.WindowLeft = this.Left;
            s.WindowTop = this.Top;
            s.WindowWidth = this.Width;
            s.WindowHeight = this.Height;
        }

        private void FilePreviewSettings_OnSaving(object sender, EventArgs e)
        {
            FilePreviewSettings s = (FilePreviewSettings)sender;
            s.IsEnabled = this.FilePreview.IsEnabled;
        }

        #endregion

        #region View Model / Window Events

        private void ViewModel_OnExtractionCompleted(object sender, MainViewModel.ExtractionEventArgs e)
        {
            switch (e.State)
            {
                case ExtractionFinishedState.Failed:
                    this.ShowStatusBar($"Extraction failed.");
                    break;
                case ExtractionFinishedState.Canceled:
                    this.ShowStatusBar($"Extraction was canceled.");
                    break;
                case ExtractionFinishedState.Succeed:
                    if (e.ExtractedCount == 1)
                    {
                        this.ShowStatusBarWithButton($"File \"{ e.ExtractedFileName }\" extracted.", "Open file folder",
                            () => OpenAppUtil.ExplorerOpenPath(this, e.ExtractedFileName, true));
                    }
                    else
                    {
                        this.ShowStatusBarWithButton($"{ e.ExtractedCount } files were extracted.", "Open folder",
                            () => OpenAppUtil.ExplorerOpenPath(this, e.DestinationFolder, false));
                    }
                    break;
            }
        }

        private void ViewModel_OnArchiveOpened(object sender, EventArgs e)
        {
            //archiveFilesFilter = (CollectionView)CollectionViewSource.GetDefaultView(this.ArchiveFilesList.ItemsSource);
            //archiveFilesFilter.Filter = ArchiveFileFilter;

            FileListView.Tag = 0;
            FileListView.ItemsSource = ArchiveFilePathService.GetRoots(viewModel.ArchiveInfo);

            this.UpdateTitle(viewModel.ArchiveInfo.FilePath);
            this.ShowStatusBar($"{ viewModel.ArchiveInfo.FilePath } • { viewModel.ArchiveInfo.TotalFiles } files.");
        }

        private void ViewModel_OnArchiveClosed(object sender, bool resetUI)
        {
            if (resetUI)
            {
                this.UpdateTitle();
                this.HideStatusBar();
            }

            FileListView.ItemsSource = null;
            currentItem = null;
            paths.Clear();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel.MainWindowLoaded();

            // View updates
            UpdateAssociateExtensionMenuItem();
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            this.viewModel.Cleanup();
        }

        #endregion

        #region Archive files filter

        private bool ArchiveFileFilter(object item)
        {
            if (String.IsNullOrWhiteSpace(FilterText.Text))
                return true;
            else
            {
                string filePath = (string)item;
                return filePath.IndexOf(FilterText.Text, StringComparison.OrdinalIgnoreCase) >= 0;
            }
        }

        private void FilterText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (viewModel.ArchiveInfo == null)
                return;

            //CollectionViewSource.GetDefaultView(ArchiveFilesList.ItemsSource).Refresh();

            //if (FilePreview.HasFileInQueue())
            //    SetSelectedItemFilePreview();
        }

        #endregion

        #region Commands / Logic

        private void OpenCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            viewModel.OpenArchiveWithDialog();
            e.Handled = true;
        }

        private void CloseCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = viewModel.ArchiveInfo != null;
            e.Handled = true;
        }

        private void CloseCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            viewModel.CloseArchive(true);

            GC.Collect(2);
        }

        private void ExtractCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (viewModel.ArchiveInfo == null)
            {
                e.CanExecute = false;
                e.Handled = true;
                return;
            }

            // e.CanExecute = ArchiveFilesList.SelectedIndex != -1;
            e.Handled = true;
        }

        private /* async */ void ExtractSelectedExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            //if (ArchiveFilesList.SelectedItems.Count == 1)
            //{
            //    string sel = ArchiveFilesList.SelectedItem as string;

            //    await viewModel.ExtractFileWithDialog(viewModel.ArchiveInfo.Archive.GetFileIndex(sel));
            //    e.Handled = true;
            //}
            //else if (ArchiveFilesList.SelectedItems.Count > 1)
            //{
            //    List<string> sels = ArchiveFilesList.SelectedItems.Cast<string>().ToList();
            //    List<int> ss = new List<int>();
            //    foreach (var sel in sels)
            //    {
            //        ss.Add(viewModel.ArchiveInfo.Archive.GetFileIndex(sel));
            //    }

            //    viewModel.ExtractFilesWithDialog(ss);
            //    e.Handled = true;
            //}
        }

        private void ExtractAllCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            viewModel.ExtractAllWithDialog();
            e.Handled = true;
        }

        private void ExtractAllCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = viewModel.ArchiveInfo != null;
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

        private void RecentArchivesItemExecuted(object sender, RoutedEventArgs e)
        {
            MenuItem selection = e.OriginalSource as MenuItem;

            string item = (string)selection.Header;
            if (item == null)
                return;

            if (!viewModel.OpenArchive(item))
            {
                viewModel.RemoveRecentArchive(item);
            }
        }

        private void StatusBarButton_Click(object sender, RoutedEventArgs e)
        {
            statusbarButtonAction?.Invoke();
        }

        #endregion

        #region Helper methods

        //private void SetSelectedItemFilePreview()
        //{
        //    string selection = (string)ArchiveFilesList.SelectedItem;

        //    // TODO handle multiple selection
        //    if (selection == null) return;
        //    FilePreview.SetPreview(selection);
        //}

        /// <summary>
        /// Changes main window title. Call without parameter to reset title, call with parameter to add additional string to the title.
        /// </summary>
        /// <param name="value">Additional string, which will be appended to title. Use <c>null</c> to remove additional string.</param>
        private void UpdateTitle(string value = null)
        {
            if (value == null)
                this.Title = "BA2 Explorer";
            else
                this.Title = "BA2 Explorer • " + value;
        }

        private void ShowStatusBar(string text)
        {
            if (StatusBar.Visibility != Visibility.Visible)
                StatusBar.Visibility = Visibility.Visible;

            StatusBarButton.Visibility = Visibility.Collapsed;
            StatusBarTime.Text = DateTime.Now.ToShortTimeString();
            StatusBarText.Text = text;
        }

        private void ShowStatusBarWithButton(string statusbarText, string buttonText, Action buttonClickAction)
        {
            ShowStatusBar(statusbarText);
            StatusBarButton.Visibility = Visibility.Visible;
            StatusBarButton.Content = buttonText;
            statusbarButtonAction = buttonClickAction;
        }

        private void HideStatusBar()
        {
            StatusBar.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Extension Association

        private void UpdateAssociateExtensionMenuItem()
        {
            bool associated = ExtensionAssociation.IsExtensionAssociated;
            AssociateExtensionMenuItem.Tag = associated;
            AssociateExtensionMenuItem.Header = associated ? "Unassociate archive extension" : "Associate archive extension";

            if (!UACElevationHelper.IsRunAsAdmin())
            {
                var image = new Image();
                image.Source = StockIcon.Shield;
                AssociateExtensionMenuItem.Icon = image;
            }
        }

        private void AssociateExtensionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.AssociateExtension(this))
                if (ExtensionAssociation.IsExtensionAssociated)
                    AssociateExtensionMenuItem.Header = "Unassociate archive extension";
                else
                    AssociateExtensionMenuItem.Header = "Associate archive extension";
        }

        #endregion

        private void ArchiveFilesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //FilePreview.PreviewFileName = ArchiveFilesList.SelectedItem as string;
        }

        Stack<ArchiveFilePath> paths = new Stack<ArchiveFilePath>();

        ArchiveFilePath currentItem = null;

        private void FileListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((FrameworkElement)e.Source).DataContext as ArchiveFilePath;
            if (item != null)
            {
                if (item.Type == FilePathType.Directory)
                {
                    currentItem = item;
                    int level = (int)FileListView.Tag;
                    ++level;
                    FileListView.ItemsSource = ArchiveFilePathService.GetRoots(viewModel.ArchiveInfo, currentItem, level);
                    FileListView.Tag = level;
                    paths.Push(currentItem);
                    Debug.WriteLine("Open Dir, Item {0}, Level {1} => {2}, Stack Size {3}", currentItem.Path, level - 1, level, paths.Count);
                }
                else if (item.Type == FilePathType.GoBack)
                {
                    int level = (int)FileListView.Tag;
                    Debug.Assert(level != 0); // should not happen
                    if (level == 1)
                    {
                        Debug.WriteLine("Go Back, Get Main Roots");
                        paths.Clear();
                        FileListView.Tag = 0;
                        FileListView.ItemsSource = ArchiveFilePathService.GetRoots(viewModel.ArchiveInfo);
                        currentItem = null;
                    }
                    else
                    {
                        item = paths.Pop();
                        if (currentItem == item)
                            currentItem = paths.Pop();
                        else
                            currentItem = item;
                        --level;
                        Debug.WriteLine("Go Back, Item {0}, Level {1} => {2}, Stack Size {3}", currentItem.Path, level+1, level, paths.Count);
                        FileListView.Tag = level;
                        FileListView.ItemsSource = ArchiveFilePathService.GetRoots(viewModel.ArchiveInfo, currentItem, level);
                    }
                }
                else
                {
                    MessageBox.Show("TODO");
                }
            }
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("CC");
        }
    }
}
