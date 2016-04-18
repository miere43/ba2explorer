﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ba2Explorer.View.Commands
{
    public static class AppCommands
    {
        private static RoutedUICommand extractSingle;
        public static RoutedUICommand ExtractSingle { get { return extractSingle; } }

        private static RoutedUICommand extractAll;
        public static RoutedUICommand ExtractAll { get { return extractAll; } }

        private static RoutedUICommand openSettings;
        public static RoutedUICommand OpenSettings { get { return openSettings; } }

        private static RoutedUICommand exitApp;
        public static RoutedUICommand ExitApp { get { return exitApp; } }

        static AppCommands()
        {
            Type t = typeof(AppCommands);

            extractSingle = new RoutedUICommand("ExtractSingle", "ExtractSingle", t);
            extractAll    = new RoutedUICommand("ExtractAll",    "ExtractAll",    t);
            openSettings  = new RoutedUICommand("OpenSettings",  "OpenSettings",  t);
            exitApp       = new RoutedUICommand("ExitApp",       "ExitApp",       t);
        }
    }
}
