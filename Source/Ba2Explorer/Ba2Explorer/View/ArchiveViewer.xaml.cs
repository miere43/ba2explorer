using System;
using System.Collections.Generic;
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
using Ba2Explorer.ViewModel;

namespace Ba2Explorer.View
{
    /// <summary>
    /// Interaction logic for ArchiveViewer.xaml
    /// </summary>
    public partial class ArchiveViewer : UserControl
    {
        public ArchiveViewerViewModel ViewModel { get; set; } = new ArchiveViewerViewModel();

        public ArchiveViewer()
        {
            InitializeComponent();
            this.Loaded += ArchiveViewer_Loaded;
        }

        private void ArchiveViewer_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
