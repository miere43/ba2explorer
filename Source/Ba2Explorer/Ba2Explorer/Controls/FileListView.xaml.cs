using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public ArchiveInfo Archive
        {
            get { return (ArchiveInfo)GetValue(ArchiveProperty); }
            set
            {
                Reset();
                SetValue(ArchiveProperty, value);
                if (value != null)
                    LoadTopLevelHierarchy();
            }
        }

        public static readonly DependencyProperty ArchiveProperty =
            DependencyProperty.Register("Archive", typeof(ArchiveInfo), typeof(FileListView), new PropertyMetadata(null));

        #endregion

        /// <summary>
        /// Opened paths hierarchy.
        /// </summary>
        Stack<ArchiveFilePath> m_paths = new Stack<ArchiveFilePath>();

        /// <summary>
        /// We view this item's children now.
        /// </summary>
        ArchiveFilePath m_currentItem = null;

        /// <summary>
        /// Current level in file hierarchy
        /// </summary>
        int m_currentLevel = -1; // TODO make use of it instead of Tag property

        public FileListView()
        {
            InitializeComponent();
        }

        private void Reset()
        {
            m_paths.Clear();
            m_currentItem = null;
            m_currentLevel = -1;
        }

        private void LoadTopLevelHierarchy()
        {
            FileView.Tag = 0;
            FileView.ItemsSource = ArchiveFilePathService.GetRoots(Archive);
        }

        private void MoveHierarchy(ArchiveFilePath item)
        {
            if (item != null)
            {
                if (item.Type == FilePathType.Directory)
                {
                    m_currentItem = item;
                    int level = (int)FileView.Tag;
                    ++level;
                    FileView.ItemsSource = ArchiveFilePathService.GetRoots(Archive, m_currentItem, level);
                    FileView.Tag = level;
                    m_paths.Push(m_currentItem);
                    Debug.WriteLine("Open Dir, Item {0}, Level {1} => {2}, Stack Size {3}", m_currentItem.Path, level - 1, level, m_paths.Count);
                }
                else if (item.Type == FilePathType.GoBack)
                {
                    int level = (int)FileView.Tag;
                    Debug.Assert(level != 0); // should not happen
                    if (level == 1)
                    {
                        Debug.WriteLine("Go Back, Get Main Roots");
                        m_paths.Clear();
                        FileView.Tag = 0;
                        FileView.ItemsSource = ArchiveFilePathService.GetRoots(Archive);
                        m_currentItem = null;
                    }
                    else
                    {
                        item = m_paths.Pop();
                        if (m_currentItem == item)
                            m_currentItem = m_paths.Pop();
                        else
                            m_currentItem = item;
                        --level;
                        Debug.WriteLine("Go Back, Item {0}, Level {1} => {2}, Stack Size {3}", m_currentItem.Path, level + 1, level, m_paths.Count);
                        FileView.Tag = level;
                        FileView.ItemsSource = ArchiveFilePathService.GetRoots(Archive, m_currentItem, level);
                    }
                }
                else
                {
                    MessageBox.Show("TODO");
                }
            }
        }

        private void FileListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((FrameworkElement)e.Source).DataContext as ArchiveFilePath;
            if (item != null)
                MoveHierarchy(item);
        }
    }
}
