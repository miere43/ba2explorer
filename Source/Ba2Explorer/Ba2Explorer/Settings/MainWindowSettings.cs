using Nett;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ba2Explorer.Settings
{
    public class MainWindowSettings : AppSettingsBase
    {
        [TomlComment("Window X position from left")]
        public double WindowLeft { get; set; } = 32;

        [TomlComment("Window Y position from top")]
        public double WindowTop { get; set; } = 32;

        public double WindowWidth { get; set; } = 525;

        public double WindowHeight { get; set; } = 350;

        [TomlComment("Should window be located above all opened windows? (bool)")]
        public bool Topmost { get; set; } = false;

        public List<string> LatestFiles = null;

        /// <summary>
        /// Called by AppSettings class when settings are loaded.
        /// </summary>
        public override void Loaded()
        {
            if (WindowWidth <= 0)
                WindowWidth = 525;
            if (WindowHeight <= 0)
                WindowHeight = 350;
            if (WindowLeft < -WindowWidth)
                WindowLeft = 32;
            if (WindowTop < -WindowHeight)
                WindowTop = 32;
            if (LatestFiles == null)
                LatestFiles = new List<string>();

            base.Loaded();
        }
    }
}
