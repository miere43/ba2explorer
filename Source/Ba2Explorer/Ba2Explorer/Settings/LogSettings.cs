using Nett;

namespace Ba2Explorer.Settings
{
    internal class LogSettings : AppSettingsBase
    {
        public bool LoggerEnabled { get; set; } = true;

        public string LogFilePath { get; set; } = "app.log";

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
