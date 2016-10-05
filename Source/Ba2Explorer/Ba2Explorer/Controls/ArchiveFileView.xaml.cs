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
using System.Windows.Media;
using Ba2Explorer.Service;
using Ba2Explorer.Utility;
using Ba2Explorer.View;
using Ba2Explorer.ViewModel;

namespace Ba2Explorer.Controls
{
    /// <summary>
    /// Interaction logic for FileListView.xaml
    /// </summary>
    public partial class ArchiveFileView : UserControl
    {
        #region Dependency Properties

        /// <summary>
        /// Gets or sets file selected in list view or returns null if nothing selected.
        /// </summary>
        public ArchiveFilePath SelectedItem
        {
            get { return (ArchiveFilePath)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(ArchiveFilePath), typeof(ArchiveFileView));

        public ObservableCollection<ArchiveFilePath> SelectedItems
        {
            get { return (ObservableCollection<ArchiveFilePath>)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register(nameof(SelectedItems), typeof(IList<ArchiveFilePath>), typeof(ArchiveFileView));

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

        private TreeViewItem m_rootItem = null;

        private ArchiveFilePath m_rootFilePath;

        private ObjectPool<ArchiveFilePath> m_pathsPool = new ObjectPool<ArchiveFilePath>(10);

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
                    new ArchiveFilePath() { Type = FilePathType.Directory, DisplayPath = "Some Directory" },
                    new ArchiveFilePath() { Type = FilePathType.File, DisplayPath = "Some File" }
                };
                return;
            }

            SelectedItems = new ObservableCollection<ArchiveFilePath>();
            m_rootFilePath = m_pathsPool.Take();
            m_rootFilePath.Children = new ObservableCollection<ArchiveFilePath>();
        }

        private void Reset()
        {
            FileTreeView.IsEnabled = false;
            FileListView.IsEnabled = false;

            SelectedItem = null;
            SelectedItems.Clear();
            m_filePaths.Clear();
            m_rootItem = null;
            FileListView.ItemsSource = null;
            ReturnChildrenPathsToPool(m_rootFilePath);
            m_rootFilePath.Reset();
            m_pathsPool.ResetItemPointers();

            FileTreeView.IsEnabled = true;
            FileListView.IsEnabled = true;
        }

        private void ReturnChildrenPathsToPool(ArchiveFilePath root)
        {
            foreach (var child in root.Children)
            {
                if (child.Type == FilePathType.Directory && child.Children != null)
                {
                    ReturnChildrenPathsToPool(child);
                }
                m_pathsPool.Return(child);
            }
        }

        private void LoadTopLevelHierarchy()
        {
            m_rootFilePath.DisplayPath = Archive.FileName;
            m_rootFilePath.RealPath = "\\";
            m_rootFilePath.Type = FilePathType.Directory;

            List<ArchiveFilePath> rootDirs = new List<ArchiveFilePath>();
            ArchiveFilePathService.GetRootDirectories(rootDirs, Archive.Archive, m_pathsPool);
            foreach (var path in rootDirs)
            {
                if (path.Type == FilePathType.Directory)
                    path.DiscoverChildren(Archive.Archive, m_pathsPool);
                path.Parent = m_rootFilePath;
                m_rootFilePath.Children.Add(path);
            }

            FilePaths.Add(m_rootFilePath);
            FileListView.ItemsSource = m_rootFilePath.Children;

            var itemGen = FileTreeView.ItemContainerGenerator;
            if (itemGen.Status != GeneratorStatus.ContainersGenerated)
            {
                itemGen.GenerateBatches().Dispose();
            }

            FileTreeView.UpdateLayout();
            m_rootItem = (itemGen.ContainerFromIndex(0) as TreeViewItem);
            m_rootItem.IsSelected = true;
            m_rootItem.IsExpanded = true;
        }

        private void FileTree_ItemSelected(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)e.OriginalSource;
            ArchiveFilePath selectedFilePath = (ArchiveFilePath)item.DataContext;
			ArchiveFilePath selectedDirectory = m_selectedDirectory;

            if (selectedFilePath == m_selectedDirectory) return;
            if (selectedFilePath.Type == FilePathType.File)
            {
                if (selectedFilePath.Parent == null) return;
                if (selectedFilePath.Parent == m_selectedDirectory)
                {
                    FileListView.SelectedItem = selectedFilePath;
                }
				selectedDirectory = selectedFilePath.Parent;
            }
			else if (selectedFilePath.Type == FilePathType.Directory)
			{
				selectedDirectory = selectedFilePath;
			}

            m_selectedDirectory = selectedDirectory;
            m_selectedDirectoryItem = item;
            FileListView.ItemsSource = selectedDirectory.Children;

            this.SelectedItems.Clear();
            this.SelectedItems.Add(selectedFilePath);
            this.SelectedItem = selectedFilePath;
        }

        private void FileTree_ItemExpanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)e.OriginalSource;

            foreach (var filePathObject in item.Items)
            {
                ArchiveFilePath filePath = (ArchiveFilePath)filePathObject;
                if (filePath.Type == FilePathType.Directory && (filePath.Children == null || filePath.Children.Count == 0))
                {
                    filePath.DiscoverChildren(Archive.Archive, m_pathsPool);
                }
            }
        }

        private void ListViewOpenItem(ArchiveFilePath item)
        {
            Contract.Requires(item != null);

            if (item.Type == FilePathType.Directory)
            {
                TreeViewItem treeItem = (TreeViewItem)m_selectedDirectoryItem.ItemContainerGenerator.ContainerFromItem(item);
                if (treeItem == null)
                {
                    if (m_selectedDirectoryItem.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                    {
                        // Selected Directory TreeViewItem wasn't expanded yet and children TreeViewItem's was not generated yet, which
                        // means that call to ItemContainerGenerator.ContainerFromItem(item) will return null.
                        // We force item generation, container expansion and layout invalidation so call to that method returns TreeViewItem
                        // as excepted.
                        m_selectedDirectoryItem.IsExpanded = true;
                        m_selectedDirectoryItem.ItemContainerGenerator.GenerateBatches().Dispose();
                    }

                    m_selectedDirectoryItem.BringIntoView();
                    m_selectedDirectoryItem.UpdateLayout();
                    treeItem = (TreeViewItem)m_selectedDirectoryItem.ItemContainerGenerator.ContainerFromItem(item);
                }
				if (treeItem != null)
				{
					if (m_selectedDirectory != null && !m_selectedDirectoryItem.IsExpanded) { 
						m_selectedDirectoryItem.IsExpanded = true;
					}
					treeItem.IsSelected = true;
					treeItem.IsExpanded = true;
				}
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

        private void FileList_ItemMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((FrameworkElement)e.Source).DataContext as ArchiveFilePath;
            if (item != null)
                ListViewOpenItem(item);
        }

        private void FileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedItems.Clear();
            foreach (var oitem in FileListView.SelectedItems)
            {
                ArchiveFilePath item = (ArchiveFilePath)oitem;
                if (item == null)
                    continue;
                SelectedItems.Add(item);
            }

			SelectedItem = SelectedItems.Count == 0 ? null : SelectedItems[0];
        }

        private void FileList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var item = (ArchiveFilePath)FileListView.SelectedItem;
                if (item != null)
                    ListViewOpenItem(item);
                e.Handled = true;
            }
        }

        private void FileTree_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
            }
        }

        static TreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
                source = VisualTreeHelper.GetParent(source);

            return source as TreeViewItem;
        }
    }
}
