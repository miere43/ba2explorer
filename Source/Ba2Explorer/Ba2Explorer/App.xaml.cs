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
using Microsoft.Win32;
using System.Security;
using Ba2Explorer.Utility;

namespace Ba2Explorer
{
    public partial class App : Application
    {
        internal static ILogger Logger { get; private set; }

        public App()
        {
            AppSettings.Load("prefs.toml");

            if (AppSettings.Instance.Logger.IsEnabled)
            {
                FileStream file = null;
                try
                {
                    file = File.Open(AppSettings.Instance.Logger.LogFilePath, FileMode.Append);

                    Logger = new FileLogger(file)
                    {
                        LogMaxSize = AppSettings.Instance.Logger.LogMaxSize
                    };
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Error while setting up logger: { e.Message }", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    Logger = new NullLogger();
                }
            }
            else
            {
                Logger = new NullLogger();
            }

            Logger = Logger;

            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            this.Activated += App_Activated;
        }

        private void App_Activated(object sender, EventArgs unused)
        {
//#if DEBUG
//            if (!Debugger.IsAttached)
//                Debugger.Launch();
//#endif
            HandleArguments();
            this.Activated -= App_Activated;
        }

        private void HandleArguments()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Contains("/associate-extension"))
            {
                ExtensionAssociation.TryAssociate(null, alreadyTriedToAssociate: true);
            }
            else if (args.Contains("/unassociate-extension"))
            {
                ExtensionAssociation.TryUnassociate(null, alreadyTriedToUnassociate: true);
            }
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (Logger != null)
            {
                try
                {
                    Logger.Log(LogPriority.Error, "!!! Unhandled exception, dispatcher: {0}", e.Dispatcher.ToString());
                    LogException(Logger, e.Exception);
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
            if (Logger != null)
                Logger.Log(LogPriority.Info, "App closed");

            Logger?.Dispose();
            Logger = null;
            this.DispatcherUnhandledException -= App_DispatcherUnhandledException;
        }
    }
}
