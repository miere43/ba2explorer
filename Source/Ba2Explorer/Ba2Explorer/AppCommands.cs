using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ba2Explorer
{
    public static class AppCommands
    {
        private static RoutedUICommand extractSingle;

        public static RoutedUICommand ExtractSingle
        {
            get {  return extractSingle; }
        }

        static AppCommands()
        {
            extractSingle = new RoutedUICommand("ExtractSingle", "ExtractSingle", typeof(AppCommands));
        }
    }
}
