using Ba2Explorer.Logging;
using Ba2Explorer.Settings;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Ba2Explorer
{
    public partial class App : Application
    {
        internal ILogger logger;
        internal static ILogger Logger { get; private set; }

        public App()
        {
            AppSettings.Load("prefs.toml");

            if (AppSettings.Instance.Logger.LoggerEnabled)
            {
                logger = new FileLogger(File.Open(AppSettings.Instance.Logger.LogFilePath, FileMode.Append))
                {
                    LogMaxSize = AppSettings.Instance.Logger.LogMaxSize
                };
            }
            else
            {
                logger = new NullLogger();
            }

            Logger = logger;
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            AppSettings.Save("prefs.toml");
            Logger.Log("App closed", LogPriority.Info);

            logger.Dispose();
        }
    }
}
