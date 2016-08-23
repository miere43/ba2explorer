using Ba2Explorer.Settings;
using System;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Controls;

namespace Ba2Explorer.Controls
{
    /// <summary>
    /// Represents a control which is capable of playing
    /// various audio tracks. Supports WAV only.
    /// </summary>
    public partial class AudioPlayer : UserControl
    {
        private Stream soundSource;
        public Stream SoundSource
        {
            get
            {
                return soundSource;
            }
            set
            {
                soundSource = value;
                SoundSourceChanged();    
            }
        }

        private SoundPlayer soundPlayer;

        public AudioPlayer()
        {
            InitializeComponent();
            soundPlayer = new SoundPlayer();

            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                this.AutoplaySoundCheckbox.IsChecked =
                    AppSettings.Instance.FilePreview.SoundPlayerAutoplaySounds;

                AppSettings.Instance.FilePreview.OnSaving += FilePreviewSettingsBeforeSave;
            } 

            SoundSourceChanged(); // No source by default.
        }

        ~AudioPlayer()
        {
            AppSettings.Instance.FilePreview.OnSaving -= FilePreviewSettingsBeforeSave;
        }

        /// <summary>
        /// Stops audio playback.
        /// </summary>
        public void StopAudio()
        {
            soundPlayer.Stop();
        }

        #region Private methods

        private void FilePreviewSettingsBeforeSave(object sender, EventArgs e)
        {
            AppSettings.Instance.FilePreview.SoundPlayerAutoplaySounds
                = AutoplaySoundCheckbox.IsChecked.HasValue ? AutoplaySoundCheckbox.IsChecked.Value : false;
        }

        /// <summary>
        /// Raised when SoundSource property changed.
        /// </summary>
        private void SoundSourceChanged()
        {
            soundPlayer.Stop();

            if (soundSource == null)
                return;

            soundPlayer.Stream = soundSource;

            soundPlayer.Load();

            if (AutoplaySoundCheckbox.IsChecked.HasValue &&
                AutoplaySoundCheckbox.IsChecked.Value == true)
            {
                soundPlayer.Play();
            }
        }

        private void PlayButtonClicked(object sender, RoutedEventArgs e)
        {
            soundPlayer.Play();
        }

        private void StopButtonClicked(object sender, RoutedEventArgs e)
        {
            StopAudio();
        }

        #endregion
    }
}
