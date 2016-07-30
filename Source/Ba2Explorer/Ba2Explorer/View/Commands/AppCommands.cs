using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ba2Explorer.View.Commands
{
    public static class AppCommands
    {
        public static RoutedUICommand ExtractSelected { get; private set; }

        public static RoutedUICommand ExtractAll { get; private set; }

        public static RoutedUICommand OpenSettings { get; private set; }

        public static RoutedUICommand ExitApp { get; private set; }

        static AppCommands()
        {
            Type t = typeof(AppCommands);

            ExtractSelected = new RoutedUICommand("ExtractSelected", "ExtractSelected", t);
            ExtractAll      = new RoutedUICommand("ExtractAll",      "ExtractAll",      t);
            OpenSettings    = new RoutedUICommand("OpenSettings",    "OpenSettings",    t);
            ExitApp         = new RoutedUICommand("ExitApp",         "ExitApp",         t);
            ApplicationCommands.Close.InputGestures.Add(new KeyGesture(Key.W, ModifierKeys.Control));
        }
    }
}
