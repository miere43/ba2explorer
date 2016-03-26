using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ba2Explorer
{
    public class ArchiveFilename
    {
        public string Filename { get; set; }

        public ObservableCollection<ArchiveFilename> Filenames { get; set; }

        public ArchiveFilename(string name)
        {
            Filename = name;
            Filenames = new ObservableCollection<ArchiveFilename>();
        }

        public ArchiveFilename(string name, ObservableCollection<ArchiveFilename> names)
        {
            Filename = name;
            Filenames = names;
        }

        public void Add(string name)
        {
            Filenames.Add(new ArchiveFilename(name));
        }
    }
}
