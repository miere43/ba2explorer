using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Ba2Explorer.Service;
using Ba2Explorer.View;
using Ba2Explorer.ViewModel;

namespace Ba2Explorer.Controls
{
    public class FileListItem
    {
        public FilePathType Type { get; set; }

        public string Path { get; set; }
    }

    /// <summary>
    /// Interaction logic for FileListView.xaml
    /// </summary>
    public partial class ArchiveFileView : UserControl
    {
        #region Dependency Properties

        /// <summary>
        /// Gets or sets file selected in list view. Returns null if nothing selected or selected item is not file.
        /// </summary>
        public FileListItem SelectedItem
        {
            get { return (FileListItem)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(FileListItem), typeof(ArchiveFileView));

        public IList<FileListItem> SelectedItems
        {
            get { return (IList<FileListItem>)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register(nameof(SelectedItems), typeof(IList<FileListItem>), typeof(ArchiveFileView));

        public ArchiveInfo Archive
        {
            get { return (ArchiveInfo)GetValue(ArchiveProperty); }
            set { SetValue(ArchiveProperty, value); }
        }

        public static readonly DependencyProperty ArchiveProperty =
            DependencyProperty.Register(nameof(Archive), typeof(ArchiveInfo), typeof(ArchiveFileView), 
               new FrameworkPropertyMetadata(propertyChangedCallback: ArchivePropertyChanged));

        #endregion

        private ObservableCollection<ArchiveFilePath> m_filePaths = new ObservableCollection<ArchiveFilePath>();
        public ObservableCollection<ArchiveFilePath> FilePaths { get { return m_filePaths; } }

        private ArchiveFilePath m_selectedDirectory = null;

        private TreeViewItem m_selectedDirectoryItem = null;

        private static void ArchivePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (ArchiveFileView)d;
            self.Reset();
            if (self.Archive != null)
                self.LoadTopLevelHierarchy();
        }

        public ArchiveFileView()
        {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                FileListView.ItemsSource = new ObservableCollection<ArchiveFilePath>()
                {
                    new ArchiveFilePath() { Type = FilePathType.GoBack, DisplayPath = "..." },
                    new ArchiveFilePath() { Type = FilePathType.Directory, DisplayPath = "Some Directory" },
                    new ArchiveFilePath() { Type = FilePathType.File, DisplayPath = "Some File" }
                };
                return;
            }

            SelectedItems = new List<FileListItem>();
        }

        private void Reset()
        {
            PathLabel.Content = "";
            SelectedItem = null;
            SelectedItems.Clear();
            m_filePaths.Clear();
        }

        private void LoadTopLevelHierarchy()
        {
            ArchiveFilePath root = new ArchiveFilePath();
            root.DisplayPath = Archive.FileName;
            root.Children = new ObservableCollection<ArchiveFilePath>();
            root.Type = FilePathType.Directory;

            List<ArchiveFilePath> rootDirs = new List<ArchiveFilePath>();
            ArchiveFilePathService.GetRootDirectories(rootDirs, Archive.Archive);
            foreach (var path in rootDirs)
            {
                if (path.Type == FilePathType.Directory)
                    path.DiscoverChildren(Archive.Archive);
                root.Children.Add(path);
            }

            FilePaths.Add(root);
            FileListView.ItemsSource = root.Children;

            TreeViewItem rootItem = (FileTreeView.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem);
            rootItem.IsSelected = true;
            rootItem.IsExpanded = true;
        }

        private void Item_Selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)e.OriginalSource;
            ArchiveFilePath selectedFilePath = (ArchiveFilePath)item.DataContext;

            if (selectedFilePath.Type == FilePathType.Directory)
            {
                m_selectedDirectory = selectedFilePath;
                m_selectedDirectoryItem = item;
                FileListView.ItemsSource = selectedFilePath.Children;
            }
        }

        private void FileTree_ItemExpanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)e.OriginalSource;

            foreach (var filePathObject in item.Items)
            {
                ArchiveFilePath filePath = (ArchiveFilePath)filePathObject;
                if (filePath.Type == FilePathType.Directory && filePath.Children == null)
                {
                    filePath.DiscoverChildren(Archive.Archive);
                }
            }
        }

        private void GoBack()
        {
        }

        private void ListViewOpenItem(ArchiveFilePath item)
        {
            Contract.Requires(item != null);

            if (item.Type == FilePathType.Directory)
            {
                if (m_selectedDirectoryItem.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                {
                    // Selected Directory TreeViewItem wasn't expanded yet and children TreeViewItem's was not generated yet, which
                    // means that call to ItemContainerGenerator.ContainerFromItem(item) will return null.
                    // We force item generation, container expansion and layout invalidation so call to that method returns TreeViewItem
                    // as excepted.
                    m_selectedDirectoryItem.IsExpanded = true;
                    m_selectedDirectoryItem.ItemContainerGenerator.GenerateBatches().Dispose();
                    m_selectedDirectoryItem.UpdateLayout();
                }

                TreeViewItem treeItem = (TreeViewItem)m_selectedDirectoryItem.ItemContainerGenerator.ContainerFromItem(item);
                treeItem.IsExpanded = true;
                treeItem.IsSelected = true;
            }
            else if (item.Type == FilePathType.GoBack)
            {
                GoBack();
            }
            else if (item.Type == FilePathType.File)
            {
                // do nothing
            }
            else
            {
                throw new NotSupportedException($"{ item.Type } is not supported.");
            }

            FileListView.Items.Refresh();
            //UpdatePathLabel();
        }

        private void UpdatePathLabel()
        {
            StringBuilder b = new StringBuilder("\\");
            //foreach (var path in m_paths)
            //{
            //    b.Append(path.DisplayPath);
            //    b.Append('\\');
            //}
            PathLabel.Content = b.ToString();
        }

        private void FileViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((FrameworkElement)e.Source).DataContext as ArchiveFilePath;
            if (item != null)
                ListViewOpenItem(item);
        }

        private void FileView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedItems.Clear();
            //StringBuilder b = new StringBuilder();
            foreach (var oitem in FileListView.SelectedItems)
            {
                ArchiveFilePath item = (ArchiveFilePath)oitem;
                if (item == null)
                {
                    continue;
                }
                //b.Clear();

                //for (int i = 0; i < m_paths.Count; ++i)
                //{
                //    b.Append(m_paths[i].DisplayPath);
                //    b.Append('\\');
                //}
                //b.Append(item.DisplayPath);
                SelectedItems.Add(new FileListItem()
                {
                    Type = item.Type,
                    Path = item.DisplayPath
                });
            }

            if (SelectedItems.Count == 0)
            {
                SelectedItem = null;
            }
            else
            {
                SelectedItem = SelectedItems[0];
            }
        }

        private void FileView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var item = (ArchiveFilePath)FileListView.SelectedItem;
                if (item != null)
                    ListViewOpenItem(item);
            }
            else if (e.Key == Key.Back)
            {
                GoBack();

                FileListView.Items.Refresh();
                UpdatePathLabel();
            }
        }
    }
}
