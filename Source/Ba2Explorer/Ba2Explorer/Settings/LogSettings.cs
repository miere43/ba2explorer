using System.IO;
using Nett;

namespace Ba2Explorer.Settings
{
    internal class LogSettings : AppSettingsBase
    {
        public static readonly string DefaultLogFilePath = "app.log";

        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Not normalized. Check before use.
        /// </summary>
        public string LogFilePath { get; set; } = DefaultLogFilePath;

        [TomlComment("Log max size in bytes")]
        public int LogMaxSize { get; set; } = 1024 * 1024;

        public LogSettings()
        {

        }

        public override void Loaded()
        {
            if (LogMaxSize < 0)
                LogMaxSize = 0;
            base.Loaded();
        }
    }
}
