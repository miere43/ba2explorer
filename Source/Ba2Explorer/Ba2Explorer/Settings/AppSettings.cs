using Nett;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ba2Explorer.Settings
{
    internal class AppSettings
    {
        internal static AppSettings Instance { get; private set; }

        public GlobalSettings Global { get; set; }

        public MainWindowSettings MainWindow { get; set; }

        public AppSettings()
        {
            if (Instance != null)
                throw new InvalidOperationException("Settings already created.");

            Instance = this;
        }

        public static AppSettings Load(string path)
        {
            if (Instance != null)
                throw new InvalidOperationException("Settings already loaded.");

            TomlConfig config = TomlConfig.Create((builder) =>
            {
                builder.AllowImplicitConversions(TomlConfig.ConversionLevel.DotNetImplicit);
            });
            var settings = Toml.ReadFile<AppSettings>(path, config);
            settings.Loaded();

            return settings;
        }

        public static void Save(string path)
        {
            Toml.WriteFile<AppSettings>(AppSettings.Instance, path);
        }

        private void Saving()
        {
            Global.Saving();
            MainWindow.Saving();
        }

        private void Loaded()
        {
            if (Global == null)
                Global = new GlobalSettings();
            Global.Loaded();

            if (MainWindow == null)
                MainWindow = new MainWindowSettings();
            MainWindow.Loaded();
        }
    }
}
