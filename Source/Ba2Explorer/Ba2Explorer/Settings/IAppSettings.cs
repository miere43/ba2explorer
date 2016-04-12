using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ba2Explorer.Settings
{
    public interface IAppSettings
    {
        event EventHandler OnSaving;

        event EventHandler OnLoaded;

        void Saving();

        void Loaded();
    }
}
