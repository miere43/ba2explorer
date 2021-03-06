﻿namespace Ba2Explorer.Settings
{
    /// <summary>
    /// Represents settings for FilePreviewControl in MainWindow.
    /// </summary>
    internal class FilePreviewSettings : AppSettingsBase
    {
        public bool IsEnabled { get; set; } = true;

        public bool SoundPlayerAutoplaySounds { get; set; } = false;

        public FilePreviewSettings()
        {

        }
    }
}
