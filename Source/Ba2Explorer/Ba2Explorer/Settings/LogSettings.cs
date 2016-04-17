using Nett;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ba2Explorer.Settings
{
    public class LogSettings : AppSettingsBase
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
