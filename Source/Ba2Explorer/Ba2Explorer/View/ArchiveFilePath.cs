using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ba2Explorer.View
{
    public enum FilePathType
    {
        GoBack,
        Directory,
        File
    }

    public class ArchiveFilePath
    {
        public FilePathType Type { get; set; }

        public string Path { get; set; }
    }
}
