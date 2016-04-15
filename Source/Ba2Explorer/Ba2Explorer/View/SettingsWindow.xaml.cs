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
using System.Windows.Shapes;
using Ba2Explorer.Settings;

namespace Ba2Explorer.View
{
    /// <summary>
    /// Settings window.
    /// </summary>
    /// <see cref="Ba2Explorer.Settings.AppSettings"/>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();

            this.Loaded += SettingsWindow_Loaded;
        }

        private void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var settings = AppSettings.Instance;

            foreach (var property in settings.GetType().GetProperties())
            {
                if (property.PropertyType.BaseType != typeof(AppSettingsBase) &&
                    property.PropertyType.BaseType != typeof(IAppSettings))
                    continue;

                TreeViewItem propItem = new TreeViewItem();
                propItem.Header = property.Name;
                SettingsTreeView.Items.Add(propItem);
            }

            SettingsTreeView.InvalidateVisual();
        }
    }
}
