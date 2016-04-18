using Nett;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ba2Explorer.Settings
{
    internal class MainWindowSettings : AppSettingsBase
    {
        public double WindowLeft { get; set; } = 32;

        public double WindowTop { get; set; } = 32;

        public double WindowWidth { get; set; } = 525;

        public double WindowHeight { get; set; } = 350;

        [TomlComment("Should window be located above all opened windows? (bool)")]
        public bool Topmost { get; set; } = false;

        public List<string> LatestFiles { get; set; } = null;

        private const int maxLatestFiles = 5;

        public void PushLatestFile(string filePath, IList<string> to)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));
            if (to == null)
                throw new ArgumentNullException(nameof(to));

            if (LatestFiles == null)
                LatestFiles = new List<string>();

            PushLatestFileInternal(filePath, this.LatestFiles);
            PushLatestFileInternal(filePath, to);
        }

        private void PushLatestFileInternal(string filePath, IList<string> to)
        {
            int sameIndex = to.IndexOf(filePath);
            if (sameIndex == -1)
            {
                if (to.Count >= maxLatestFiles)
                    to.RemoveAt(0);
            }
            else
            {
                to.RemoveAt(sameIndex);
            }

            to.Add(filePath);
        }

        /// <summary>
        /// Rethieves latest files as ObservableCollection.
        /// </summary>
        public ObservableCollection<string> GetLatestFiles()
        {
            return new ObservableCollection<string>(this.LatestFiles);
        }

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
