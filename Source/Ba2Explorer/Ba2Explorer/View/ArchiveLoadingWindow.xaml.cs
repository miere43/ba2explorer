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
using Ba2Explorer.ViewModel;

namespace Ba2Explorer.View
{
    /// <summary>
    /// Interaction logic for ArchiveLoadingWindow.xaml
    /// </summary>
    public partial class ArchiveLoadingWindow : Window
    {
        private MainViewModel m_owner;

        private Task m_waitTask;

        public ArchiveLoadingWindow(MainViewModel owner, Task waitTask)
        {
            m_owner = owner;
            m_waitTask = waitTask;

            IsVisibleChanged += ArchiveLoadingWindow_IsVisibleChanged;       

            InitializeComponent();
        }

        private void ArchiveLoadingWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Debug.WriteLine("vis changed");
            IsVisibleChanged -= ArchiveLoadingWindow_IsVisibleChanged;

            Debug.WriteLine("v before: {0}", m_waitTask.Status);
            m_waitTask.Wait();
            Debug.WriteLine("v after: {0}", m_waitTask.Status);
            Close();
        }
    }
}
