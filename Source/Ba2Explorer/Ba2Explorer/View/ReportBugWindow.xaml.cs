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
using System.Windows.Shapes;

namespace Ba2Explorer.View
{
    /// <summary>
    /// Interaction logic for ReportBugWindow.xaml
    /// </summary>
    public partial class ReportBugWindow : Window
    {
        public ReportBugWindow()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start((sender as Hyperlink).NavigateUri.AbsoluteUri)?.Dispose();
            e.Handled = true;
        }
    }
}
