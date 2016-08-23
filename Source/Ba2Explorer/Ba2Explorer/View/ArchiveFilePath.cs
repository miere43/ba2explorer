using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ba2Explorer.Service;
using Ba2Tools;
using System.Diagnostics;
using System.Windows.Data;
using Ba2Explorer.Utility;

namespace Ba2Explorer.View
{
    public enum FilePathType
    {
        Directory,
        File
    }

    [DebuggerDisplay("{Type}, PathComponent = {DisplayPath}")]
    public sealed class ArchiveFilePath : Poolable
    {
        private static ArchiveFilePathCustomSorter sorter = new ArchiveFilePathCustomSorter();

        public FilePathType Type { get; set; }

        public string DisplayPath { get; set; }

        public string RealPath { get; set; }

        public ObservableCollection<ArchiveFilePath> Children { get; set; }

        public ArchiveFilePath Parent { get; set; }

        public void DiscoverChildren(BA2Archive archive, ObjectPool<ArchiveFilePath> pool)
        {
            if (Children != null) return;
            Children = new ObservableCollection<ArchiveFilePath>();
            ArchiveFilePathService.DiscoverDirectoryItems(Children, archive, this, pool);

            var g = (ListCollectionView)CollectionViewSource.GetDefaultView(Children);
            g.CustomSort = sorter;
        }

        public string GetDirectoryPath()
        {
            int p = RealPath.IndexOf(DisplayPath, 0, StringComparison.OrdinalIgnoreCase);
            return RealPath.Substring(0, p + DisplayPath.Length);
        }

        public override void Reset()
        {
            if (Children != null)
            {
                Children.Clear();
            }
            Parent = null;
            DisplayPath = null;
            RealPath = null;
            // Type = meh
        }
    }
}
