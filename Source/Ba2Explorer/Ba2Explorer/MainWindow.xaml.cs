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
using System.Windows.Media.Imaging;
using System.Windows.Interop;

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

            viewModel.PropertyChanged += (sender, args) =>
            {
                if (viewModel.ArchiveInfo != null && args.PropertyName == nameof(ArchiveInfo))
                {
                    // todo string.intern(nameof(archiveinfo)) ?
                    this.FilePreview.SetArchive(viewModel.ArchiveInfo);
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

            UpdateAssociateExtensionMenuItem();
        }

        private void ViewModel_OnExtractionCompleted(object sender, MainViewModel.ExtractionEventArgs e)
        {
            if (e.IsOneFileExtracted)
            {
                this.ShowStatusBarWithButton($"File \"{ e.ExtractedFiles[0] }\" extracted.", "Open file folder",
                    () => OpenAppUtil.ExplorerOpenPath(this, e.ExtractedFiles[0], true));
            }
            else
            {
                this.ShowStatusBarWithButton($"{ e.ExtractedFiles.Count() } files were extracted.", "Open folder",
                    () => OpenAppUtil.ExplorerOpenPath(this, e.DestinationFolder, false));
            }
        }

        private void ViewModel_OnArchiveOpened(object sender, EventArgs e)
        {
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
        }

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

        internal void ShowStatusBar(string text)
        {
            if (StatusBar.Visibility != Visibility.Visible)
                StatusBar.Visibility = Visibility.Visible;

            StatusBarButton.Visibility = Visibility.Collapsed;
            StatusBarTime.Text = DateTime.Now.ToShortTimeString();
            StatusBarText.Text = text;
        }

        internal void ShowStatusBarWithButton(string statusbarText, string buttonText, Action buttonClickAction)
        {
            ShowStatusBar(statusbarText);
            StatusBarButton.Visibility = Visibility.Visible;
            StatusBarButton.Content = buttonText;
            statusbarButtonAction = buttonClickAction;
        }

        internal void HideStatusBar()
        {
            StatusBar.Visibility = Visibility.Collapsed;
        }

        private void StatusBarButton_Click(object sender, RoutedEventArgs e)
        {
            statusbarButtonAction?.Invoke();
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
            this.viewModel.Cleanup();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel.MainWindowLoaded();
            // mainViewModel.OpenArchive("D:/Games/Fallout 4/Data/Fallout4 - Sounds.ba2");
            //mainViewModel.ExtractFiles("D:/A", mainViewModel.ArchiveInfo.Files);
        }

        private void ArchiveFilesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO handle multiple selection
            if (this.ArchiveFilesList.SelectedItem == null)
                return;

            string selectedFilePath = (string)this.ArchiveFilesList.SelectedItem;

            var task = this.FilePreview.TrySetPreviewAsync(selectedFilePath);

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
            if (viewModel.ArchiveInfo == null)
                return;

            CollectionViewSource.GetDefaultView(this.ArchiveFilesList.ItemsSource).Refresh();
        }

        #endregion

        #region Commands

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

            e.CanExecute = ArchiveFilesList.SelectedIndex != -1;
            e.Handled = true;
        }

        private async void ExtractSelectedExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (ArchiveFilesList.SelectedItems.Count == 1)
            {
                string sel = ArchiveFilesList.SelectedItem as string;

                await viewModel.ExtractFileWithDialog(sel);
                e.Handled = true;
            }
            else if (ArchiveFilesList.SelectedItems.Count > 1)
            {
                IList sels = ArchiveFilesList.SelectedItems;

                viewModel.ExtractFilesWithDialog(sels.Cast<string>().ToList());
                e.Handled = true;
            }
        }

        private void ExtractAllCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            viewModel.ExtractFilesWithDialog(viewModel.ArchiveInfo.Files);
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

        #endregion

        private void RecentArchivesItemExecuted(object sender, RoutedEventArgs e)
        {
            MenuItem selection = e.OriginalSource as MenuItem;

            string item = (string)selection.Header;
            if (item == null)
                return;

            viewModel.OpenArchive(item);
        }

        private void UpdateAssociateExtensionMenuItem()
        {
            bool associated = App.IsAssociatedExtension();
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
            // TODO move to view model
            string instructions = "Pressing OK will give you a prompt to restart BA2 Explorer with admin rights, so it can " +
                                  "update extensions registry. " + Environment.NewLine + Environment.NewLine + "Press Cancel to abort.";

            // true means app is associated extension.
            if ((bool)AssociateExtensionMenuItem.Tag == true)
            {
                if (UACElevationHelper.IsRunAsAdmin())
                {
                    if (App.UnassociateBA2Extension())
                    {
                        MessageBox.Show(this, "Successfully unassociated extension.", "Success", MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        UpdateAssociateExtensionMenuItem();
                    }
                    else
                    {
                        App.Logger.Log(Logging.LogPriority.Error, "Error while unassociating extension (is admin: {0}, elevated: {1}, " +
                            "is in admin group: {2}, integrity level: {3}", UACElevationHelper.IsRunAsAdmin(),
                            UACElevationHelper.IsProcessElevated(), UACElevationHelper.IsUserInAdminGroup(),
                            UACElevationHelper.GetProcessIntegrityLevel());

                        MessageBox.Show(this, "Error occured while unassociating extension.", "Error", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
                else
                {
                    var result = NativeTaskDialog.Show(new WindowInteropHelper(this).Handle, IntPtr.Zero, "Administrator rights required",
                        "Admin rights required to unassociate archive extension from BA2 Explorer",
                        instructions, TaskDialogButtons.Ok | TaskDialogButtons.Cancel, TaskDialogIcon.Shield);

                    if (result == TaskDialogResult.Ok)
                    {
                        UACElevationHelper.Elevate("/unassociate-extension");
                    }
                }
            }
            else
            {
                if (UACElevationHelper.IsRunAsAdmin())
                {
                    if (App.AssociateBA2Extension())
                    {
                        MessageBox.Show(this, "Successfully associated extension.", "Success", MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        UpdateAssociateExtensionMenuItem();
                    }
                    else
                    {
                        App.Logger.Log(Logging.LogPriority.Error, "Error while associating extension (is admin: {0}, elevated: {1}, " +
                            "is in admin group: {2}, integrity level: {3}", UACElevationHelper.IsRunAsAdmin(),
                            UACElevationHelper.IsProcessElevated(), UACElevationHelper.IsUserInAdminGroup(),
                            UACElevationHelper.GetProcessIntegrityLevel());

                        MessageBox.Show(this, "Error occured while associating extension.", "Error", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
                else
                {
                    var result = NativeTaskDialog.Show(new WindowInteropHelper(this).Handle, IntPtr.Zero, "Administrator rights required",
                        "Administrator rights required to associate archive extension to BA2 Explorer.",
                        instructions, TaskDialogButtons.Ok | TaskDialogButtons.Cancel, TaskDialogIcon.Shield);

                    if (result == TaskDialogResult.Ok)
                    {
                        UACElevationHelper.Elevate(@"/associate-extension");
                    }
                }
            }
        }
    }
}
