using System;

namespace Ba2Explorer.Settings
{
    internal class AppSettingsBase : IAppSettings
    {
        /// <summary>
        /// Event raised just before settings being saved.
        /// </summary>
        public event EventHandler OnSaving = delegate { };

        /// <summary>
        /// Event raised just after settings being loaded.
        /// </summary>
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
