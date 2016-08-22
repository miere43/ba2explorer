using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ba2Explorer.Service;
using Ba2Tools;
using System.CodeDom.Compiler;
using System.Diagnostics;

namespace Ba2Explorer.View
{
    public enum FilePathType
    {
        GoBack,
        Directory,
        File
    }

    [DebuggerDisplay("{Type}, PathComponent = {DisplayPath}")]
    public sealed class ArchiveFilePath
    {
        public FilePathType Type { get; set; }

        public string DisplayPath { get; set; }

        public string RealPath { get; set; }

        public bool IsSelected { get; set; }

        public bool IsExpanded { get; set; }

        public ObservableCollection<ArchiveFilePath> Children { get; set; }

        public ArchiveFilePath Parent { get; set; }

        public void DiscoverChildren(BA2Archive archive)
        {
            if (Children != null) return;
            Children = new ObservableCollection<ArchiveFilePath>();
            ArchiveFilePathService.DiscoverDirectoryItems(Children, archive, this);
        }

        public string GetDirectoryPath()
        {
            int p = RealPath.IndexOf(DisplayPath, 0, StringComparison.OrdinalIgnoreCase);
            return RealPath.Substring(0, p + DisplayPath.Length);
        }
    }
}
