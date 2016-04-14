using Nett;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ba2Explorer.Settings
{
    public class AppSettings
    {
        internal static AppSettings Instance { get; private set; }

        public GlobalSettings Global { get; set; }

        public MainWindowSettings MainWindow { get; set; }

        public FilePreviewSettings FilePreview { get; set; }

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

            AppSettings settings;
            try
            {
                settings = Toml.ReadFile<AppSettings>(path, config);
            }
            catch (Exception)
            {
                settings = new AppSettings();
            }
            settings.Loaded();

            return settings;
        }

        public static void Save(string path)
        {
            if (Instance == null)
                throw new InvalidOperationException("Settings are not instantiated.");

            Instance.Saving();

            try
            {
                Toml.WriteFile<AppSettings>(AppSettings.Instance, path);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Cannot save prefs: {e.Message}");
            }
        }

        private IEnumerator<IAppSettings> SettingsPropertiesEnumerator()
        {
            foreach (var prop in typeof(AppSettings).GetProperties())
            {
                if (prop.PropertyType.BaseType != typeof(AppSettingsBase) &&
                prop.PropertyType.BaseType != typeof(IAppSettings))
                {
                    continue;
                }

                IAppSettings settings = (IAppSettings)prop.GetValue(this);
                if (settings == null)
                {
                    settings = (IAppSettings)prop.PropertyType.GetConstructor(Type.EmptyTypes).Invoke(new object[0]);
                    prop.SetValue(this, settings);
                }

                yield return settings;
            }
        }

        private void Saving()
        {
            var enumerator = SettingsPropertiesEnumerator();
            while (enumerator.MoveNext())
            {
                enumerator.Current.Saving();
            }
        }

        private void Loaded()
        {
            var enumerator = SettingsPropertiesEnumerator();
            while (enumerator.MoveNext())
            {
                enumerator.Current.Loaded();
            }

            //if (Global == null)
            //    Global = new GlobalSettings();
            //Global.Loaded();

            //if (MainWindow == null)
            //    MainWindow = new MainWindowSettings();
            //MainWindow.Loaded();

            //if (FilePreview == null)
            //    FilePreview = new FilePreviewSettings();
            //FilePreview.Loaded();
        }
    }
}
