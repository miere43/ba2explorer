using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ba2Explorer.Settings
{
    public class GlobalSettings : AppSettingsBase
    {
        public int Version { get; set; } = -1;

        public bool IsFirstLaunch { get; set; } = true;

        public override void Saving()
        {
            IsFirstLaunch = false;

            base.Saving();
        }
    }
}
