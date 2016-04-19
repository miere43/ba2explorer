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

        public List<string> RecentArchives { get; set; } = null;

        private const int maxRecentArchives = 5;

        /// <summary>
        /// Pushes archive file path to settings' internal
        /// recent archives list as well as pushing it to
        /// specified list.
        /// </summary>
        public void PushRecentArchive(string archivePath, IList<string> to)
        {
            if (archivePath == null)
                throw new ArgumentNullException(nameof(archivePath));
            if (to == null)
                throw new ArgumentNullException(nameof(to));

            if (RecentArchives == null)
                RecentArchives = new List<string>(maxRecentArchives);

            PushRecentArchiveInternal(archivePath, this.RecentArchives);
            PushRecentArchiveInternal(archivePath, to);
        }

        private void PushRecentArchiveInternal(string archivePath, IList<string> to)
        {
            int sameIndex = to.IndexOf(archivePath);
            if (sameIndex == -1)
            {
                if (to.Count >= maxRecentArchives)
                    to.RemoveAt(0);
            }
            else
            {
                to.RemoveAt(sameIndex);
            }

            to.Add(archivePath);
        }

        /// <summary>
        /// Rethieves latest files as ObservableCollection.
        /// </summary>
        public ObservableCollection<string> GetLatestFiles()
        {
            return new ObservableCollection<string>(this.RecentArchives);
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
            if (RecentArchives == null)
                RecentArchives = new List<string>(maxRecentArchives);

            base.Loaded();
        }
    }
}
