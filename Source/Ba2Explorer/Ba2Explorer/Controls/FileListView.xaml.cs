using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Ba2Explorer.Service;
using Ba2Explorer.View;
using Ba2Explorer.ViewModel;

namespace Ba2Explorer.Controls
{
    /// <summary>
    /// Interaction logic for FileListView.xaml
    /// </summary>
    public partial class FileListView : UserControl
    {
        #region Dependency Properties

        /// <summary>
        /// Gets or sets file selected in list view. Returns null if nothing selected or selected item is not file.
        /// </summary>
        public string SelectedFile
        {
            get { return (string)GetValue(SelectedFileProperty); }
            set { SetValue(SelectedFileProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedFile.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedFileProperty =
            DependencyProperty.Register(nameof(SelectedFile), typeof(string), typeof(FileListView));

        public ArchiveInfo Archive
        {
            get { return (ArchiveInfo)GetValue(ArchiveProperty); }
            set { SetValue(ArchiveProperty, value); }
        }

        public static readonly DependencyProperty ArchiveProperty =
            DependencyProperty.Register(nameof(Archive), typeof(ArchiveInfo), typeof(FileListView), 
               new FrameworkPropertyMetadata(propertyChangedCallback: ArchivePropertyChanged));

        #endregion

        private static void ArchivePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fs = (FileListView)d;
            fs.Reset();
            if (fs.Archive != null)
                fs.LoadTopLevelHierarchy();
        }

        /// <summary>
        /// Opened paths hierarchy.
        /// </summary>
        List<ArchiveFilePath> m_paths = new List<ArchiveFilePath>();

        ObservableCollection<ArchiveFilePath> m_currentPaths = new ObservableCollection<ArchiveFilePath>();

        /// <summary>
        /// Current level in file hierarchy
        /// </summary>
        int m_currentLevel = 0;

        public FileListView()
        {
            InitializeComponent();

            FileView.ItemsSource = m_currentPaths;
            ListCollectionView view = (ListCollectionView)CollectionViewSource.GetDefaultView(FileView.ItemsSource);
            view.CustomSort = new ArchiveFilePathCustomSorter();
        }

        private void Reset()
        {
            m_currentPaths.Clear();
            m_paths.Clear();
            m_currentLevel = 0;
            PathLabel.Content = "";
            SelectedFile = null;
        }

        private void LoadTopLevelHierarchy()
        {
            m_currentLevel = 0;
            ArchiveFilePathService.GetRoots(m_currentPaths, Archive);
            FileView.Items.Refresh();
            UpdatePathLabel();
        }

        private void GoBack()
        {
            if (m_currentLevel == 0)
                return;

            if (m_currentLevel == 1)
            {
                m_paths.Clear();
                m_currentLevel = 0;
                ArchiveFilePathService.GetRoots(m_currentPaths, Archive);
            }
            else
            {
                --m_currentLevel;
                m_paths.RemoveAt(m_currentLevel);
                ArchiveFilePathService.GetRoots(m_currentPaths, Archive, m_paths, m_currentLevel);
            }

            if (m_currentPaths.Count > 1)
                FileView.SelectedIndex = 1;
        }

        private void MoveHierarchy(ArchiveFilePath item)
        {
            Contract.Requires(item != null);

            if (item.Type == FilePathType.Directory)
            {
                ++m_currentLevel;
                m_paths.Add(item);
                ArchiveFilePathService.GetRoots(m_currentPaths, Archive, m_paths, m_currentLevel);
                if (m_currentPaths.Count > 1)
                    FileView.SelectedIndex = 1; // select first folder (not Go Back button)
                else if (m_currentPaths.Count == 1)
                    FileView.SelectedIndex = 0; // select Go Back button, no items in folder (not possible actually, but howerer)
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

            FileView.Items.Refresh();
            UpdatePathLabel();
        }

        private void UpdatePathLabel()
        {
            StringBuilder b = new StringBuilder("\\");
            foreach (var path in m_paths)
            {
                b.Append(path.Path);
                b.Append('\\');
            }
            PathLabel.Content = b.ToString();
        }

        private void FileViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((FrameworkElement)e.Source).DataContext as ArchiveFilePath;
            if (item != null)
                MoveHierarchy(item);
        }

        private void FileView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ArchiveFilePath item = (ArchiveFilePath)FileView.SelectedItem;
            if (item == null || item.Type != FilePathType.File)
            {
                SelectedFile = null;
                return;
            }

            StringBuilder b = new StringBuilder();
            for (int i = 0; i < m_paths.Count; ++i)
            {
                b.Append(m_paths[i].Path);
                b.Append('\\');
            }
            b.Append(item.Path);
            SelectedFile = b.ToString();
        }

        private void FileView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var item = (ArchiveFilePath)FileView.SelectedItem;
                if (item != null)
                    MoveHierarchy(item);
            }
            else if (e.Key == Key.Back)
            {
                GoBack();

                FileView.Items.Refresh();
                UpdatePathLabel();
            }
        }
    }
}
