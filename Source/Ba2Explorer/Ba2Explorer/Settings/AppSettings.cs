using Nett;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ba2Explorer.Settings
{
    /// <summary>
    /// Represents global application settings.
    /// 
    /// This is singleton class.
    /// </summary>
    internal class AppSettings
    {
        internal static AppSettings Instance { get; private set; }

        public GlobalSettings Global { get; set; }

        public MainWindowSettings MainWindow { get; set; }

        public FilePreviewSettings FilePreview { get; set; }

        [TomlComment("Logger required to identify problems and doesn't cause much speed loss.")]
        public LogSettings Logger { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppSettings"/> class. Should not be called directly from code!
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Settings already created.</exception>
        public AppSettings()
        {
            if (Instance != null)
                throw new InvalidOperationException("Settings already created.");

            Instance = this;
        }

        /// <summary>
        /// Loads settings from specified file.
        /// </summary>
        /// <param name="path">Path to settings file.</param>
        /// <exception cref="System.InvalidOperationException">Settings already loaded.</exception>
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

        /// <summary>
        /// Saves global settings to specified file.
        /// </summary>
        /// <param name="path">Path to settings file.</param>
        /// <exception cref="System.InvalidOperationException">Settings are not instantiated.</exception>
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

        /// <summary>
        /// Returns public properties of type IAppSettings of this class.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IAppSettings> GetSettingsFromProperties()
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

        /// <summary>
        /// Invokes all settings Saving() callback.
        /// </summary>
        private void Saving()
        {
            foreach (var settings in GetSettingsFromProperties())
                settings.Saving();
        }

        /// <summary>
        /// Invokes all settings Loaded() callback.
        /// </summary>
        private void Loaded()
        {
            foreach (var settings in GetSettingsFromProperties())
                settings.Loaded();
        }
    }
}
