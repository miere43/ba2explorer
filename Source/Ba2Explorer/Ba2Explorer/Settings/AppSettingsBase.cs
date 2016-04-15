using System;

namespace Ba2Explorer.Settings
{
    public class AppSettingsBase : IAppSettings
    {
        public event EventHandler OnSaving = delegate { };

        public event EventHandler OnLoaded = delegate { };

        /// <summary>
        /// Called by AppSettings class when settings are going to be saved.
        /// </summary>
        public virtual void Saving()
        {
            OnSaving(this, null);
        }

        /// <summary>
        /// Called by AppSettings class when settings are loaded.
        /// </summary>
        public virtual void Loaded()
        {
            OnLoaded(this, null);
        }
    }
}
