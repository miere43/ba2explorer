using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ba2Explorer.Settings
{
    public class AppSettingsBase : IAppSettings
    {
        public event EventHandler OnSaving = delegate { };

        public event EventHandler OnLoaded = delegate { };

        public virtual void Saving()
        {
            OnSaving(this, null);
        }

        public virtual void Loaded()
        {
            OnLoaded(this, null);
        }
    }
}
