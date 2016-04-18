using System;

namespace Ba2Explorer.Settings
{
    /// <summary>
    /// Represents a simple interface for serializing/deserializing
    /// application settings.
    /// 
    /// Should be used with AppSettings class.
    /// </summary>
    internal interface IAppSettings
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
