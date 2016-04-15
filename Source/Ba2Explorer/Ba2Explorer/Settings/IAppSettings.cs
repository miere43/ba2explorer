using System;

namespace Ba2Explorer.Settings
{
    public interface IAppSettings
    {
        event EventHandler OnSaving;

        event EventHandler OnLoaded;

        /// <summary>
        /// Called by AppSettings class when settings are going to be saved.
        /// </summary>
        void Saving();

        /// <summary>
        /// Called by AppSettings class when settings are loaded.
        /// </summary>
        void Loaded();
    }
}
