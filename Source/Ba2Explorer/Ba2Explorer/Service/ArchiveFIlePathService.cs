using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ba2Explorer.ViewModel;

namespace Ba2Explorer.Service
{
    public static class ArchiveFilePathService
    {
        private static char[] SplitChars = new char[] { '\\' };

        public static void GetRoots(ObservableCollection<ArchiveFilePath> roots, ArchiveInfo archive)
        {
            roots.Clear();
            List<int> levelDirHashes = new List<int>();

            foreach (string path in archive.Archive.FileList)
            {
                string[] names = path.Split(SplitChars);
                if (names.Length < 1)
                    continue;
                var root = names[1];
                bool isFile = names.Length <= 2;
                int rootHash = root.GetHashCode();
                if (!isFile && levelDirHashes.Contains(rootHash))
                    continue; // don't add same directory twice
                roots.Add(new ArchiveFilePath()
                {
                    Path = root,
                    Type = isFile ? FilePathType.File : FilePathType.Directory
                });
                if (!isFile)
                    levelDirHashes.Add(root.GetHashCode());
            }
        }

        /// <summary>
        /// Returns children of specified `filePath `object which is located at `level` in file hierarchy.
        /// </summary>
        /// <param name="archive"></param>
        /// <param name="filePath"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static void GetRoots(ObservableCollection<ArchiveFilePath> roots, ArchiveInfo archive, ArchiveFilePath filePath, int level)
        {
            Contract.Assert(filePath.Type == FilePathType.Directory);
            roots.Clear();
            List<int> levelDirHashes = new List<int>();

            roots.Add(new ArchiveFilePath() { Type = FilePathType.GoBack, Path = "..." });

            foreach (string path in archive.Archive.FileList)
            {
                string[] names = path.Split(SplitChars);
                if (names.Length < level + 1)
                    continue;
                if (names[level] != filePath.Path)
                    continue;
                var root = names[level + 1];
                bool isFile = names.Length <= level + 2;
                int rootHash = root.GetHashCode();
                if (!isFile && levelDirHashes.Contains(rootHash))
                    continue; // don't add same directory twice
                roots.Add(new ArchiveFilePath()
                {
                    Path = root,
                    Type = isFile ? FilePathType.File : FilePathType.Directory
                });
                if (!isFile)
                    levelDirHashes.Add(root.GetHashCode());
            }
        }
    }
}
