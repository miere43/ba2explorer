using System;
using System.Collections.Generic;
using System.IO;
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

namespace Ba2Explorer
{


    public partial class FilePreview : UserControl
    {
        private enum FileType
        {
            Unknown,
            Text
        }

        public FilePreview()
        {
            InitializeComponent();
        }

        public void SetPreviewTarget(Stream stream, string fileType)
        {
            
        }

        private FileType ResolveFileTypeFromExtension(string extension)
        {
            if (extension == "txt")
            {
                return FileType.Text;
            }

            return FileType.Unknown;
        }
    }
}
