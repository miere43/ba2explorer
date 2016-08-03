using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace Ba2Explorer.ViewModel
{
    public sealed class ArchiveViewerViewModel : ViewModelBase
    {
        public ArchiveInfo Archive { get; private set; }

        public ArchiveViewerViewModel()
        {

        }

        public void SetArchive(ArchiveInfo archive)
        {
            if (archive == null)
                throw new ArgumentNullException(nameof(archive));

            Archive = archive;
        }
    }
}
