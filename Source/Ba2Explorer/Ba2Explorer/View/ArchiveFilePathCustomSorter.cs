﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ba2Explorer.View
{
    public class ArchiveFilePathCustomSorter : IComparer, IComparer<ArchiveFilePath>
    {
        public int Compare(object x, object y)
        {
            return Compare((ArchiveFilePath)x, (ArchiveFilePath)y);
        }

        public int Compare(ArchiveFilePath x, ArchiveFilePath y)
        {
            if (x.Type == FilePathType.Directory) {
                if (y.Type == FilePathType.Directory)
                    return string.Compare(x.DisplayPath, y.DisplayPath, StringComparison.CurrentCulture);
                else if (y.Type == FilePathType.File)
                    return -1;
            }
            if (x.Type == FilePathType.File)
                if (y.Type == FilePathType.File)
                    return string.Compare(x.DisplayPath, y.DisplayPath, StringComparison.CurrentCulture);
                else if (y.Type == FilePathType.Directory)
                    return 1;
            return 0;
        }
    }
}
