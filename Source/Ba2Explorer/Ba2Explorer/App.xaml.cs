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

            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (logger != null)
            {
                try
                {
                    logger.Log(LogPriority.Error, "!!! Unhandled exception, dispatcher: {0}", e.Dispatcher.ToString());
                    LogException(logger, e.Exception);
                }
                catch { }
            }

            e.Handled = false;
        }

        internal static void LogException(ILogger logger, Exception e)
        {
            if (e == null)
            {
                logger.Log(LogPriority.Error, "Exception instance is null.");
            }
            else
            {
                logger.Log(LogPriority.Error, "!!! Catched {0}.", e.GetType().FullName);
                logger.Log(LogPriority.Error, "!!! Unhandled exception:{0}{4}source: {3}{4}target site: {1}{4}stack trace:{4}{2}{4}",
                    e.Message,
                    e.TargetSite,
                    e.StackTrace,
                    e.Source,
                    Environment.NewLine);

                if (e.InnerException != null)
                {
                    logger.Log(LogPriority.Error, "!!! Inner exception: ");
                    LogException(logger, e.InnerException);
                }
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            AppSettings.Save("prefs.toml");
            Logger.Log(LogPriority.Info, "App closed");

            logger.Dispose();
            logger = null;
            this.DispatcherUnhandledException -= App_DispatcherUnhandledException;
        }
    }
}
