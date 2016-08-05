using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
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
using Ba2Explorer.Service;
using Ba2Explorer.ViewModel;

namespace Ba2Explorer.Controls
{
    /// <summary>
    /// Interaction logic for FileListView.xaml
    /// </summary>
    public partial class FileListView : UserControl
    {
        #region Dependency Properties

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

        private void MoveHierarchy(ArchiveFilePath item)
        {
            Contract.Requires(item != null);

            if (item.Type == FilePathType.Directory)
            {
                ++m_currentLevel;
                ArchiveFilePathService.GetRoots(m_currentPaths, Archive, item, m_currentLevel);
                m_paths.Add(item);
                Debug.WriteLine("Open Dir, Item {0}, Level {1} => {2}, Stack Size {3}", item.Path, m_currentLevel - 1, m_currentLevel, m_paths.Count);
            }
            else if (item.Type == FilePathType.GoBack)
            {
                Contract.Assert(m_currentLevel != 0); // should not happen
                if (m_currentLevel == 1)
                {
                    Debug.WriteLine("Go Back, Get Main Roots");
                    m_paths.Clear();
                    m_currentLevel = 0;
                    ArchiveFilePathService.GetRoots(m_currentPaths, Archive);
                }
                else
                {
                    --m_currentLevel;
                    m_paths.RemoveAt(m_currentLevel);
                    Debug.WriteLine("Go Back, Item {0}, Level {1} => {2}, Stack Size {3}", item.Path, m_currentLevel + 1, m_currentLevel, m_paths.Count);
                    ArchiveFilePathService.GetRoots(m_currentPaths, Archive, m_paths[m_currentLevel - 1], m_currentLevel);
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

        private void FileListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((FrameworkElement)e.Source).DataContext as ArchiveFilePath;
            if (item != null)
                MoveHierarchy(item);
        }

        private void FileView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ArchiveFilePath item = (ArchiveFilePath)FileView.SelectedItem;
            if (item == null || item.Type != FilePathType.File) return;

            StringBuilder b = new StringBuilder();
            for (int i = 0; i < m_paths.Count; ++i)
            {
                b.Append(m_paths[i].Path);
                b.Append('\\');
            }
            b.Append(item.Path);
            SelectedFile = b.ToString();
        }
    }
}
