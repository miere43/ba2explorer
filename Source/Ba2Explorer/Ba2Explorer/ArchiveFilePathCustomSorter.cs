using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ba2Explorer
{
    public class ArchiveFilePathCustomSorter : IComparer, IComparer<ArchiveFilePath>
    {
        public int Compare(object x, object y)
        {
            return Compare((ArchiveFilePath)x, (ArchiveFilePath)y);
        }

        public int Compare(ArchiveFilePath x, ArchiveFilePath y)
        {
            if (x.Type == FilePathType.GoBack)
                return -1;
            if (x.Type == FilePathType.Directory) {
                if (y.Type == FilePathType.Directory)
                    return x.Path.CompareTo(y.Path);
                else if (y.Type == FilePathType.File)
                    return -1;
            }
            if (x.Type == FilePathType.File)
                if (y.Type == FilePathType.File)
                    return x.Path.CompareTo(y.Path);
                else if (y.Type == FilePathType.Directory)
                    return 1;
            return 0;
        }
    }
}
