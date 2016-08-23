using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ba2Explorer.View;
using Ba2Explorer.ViewModel;
using Ba2Tools;

namespace Ba2Explorer.Service
{
    public static class ArchiveFilePathService
    {
        private static List<string> m_names = new List<string>();

        public static void GetRootDirectories(IList<ArchiveFilePath> roots, BA2Archive archive)
        {
            roots.Clear();
            List<int> levelDirHashes = new List<int>();

            foreach (string path in archive.FileList)
            {
                SplitNames(path);
                if (m_names.Count < 1)
                    continue;
                var root = m_names[0];
                bool isFile = m_names.Count < 2;

                int rootHash = 0;
                if (!isFile)
                {
                    rootHash = StringComparer.InvariantCultureIgnoreCase.GetHashCode(root);
                    if (levelDirHashes.Contains(rootHash))
                        continue; // don't add same directory twice
                }

                roots.Add(new ArchiveFilePath()
                {
                    DisplayPath = root,
                    Type = isFile ? FilePathType.File : FilePathType.Directory,
                    RealPath = path
                });

                if (!isFile)
                    levelDirHashes.Add(rootHash);
            }
        }

        public static void DiscoverDirectoryItems(IList<ArchiveFilePath> output, BA2Archive archive, ArchiveFilePath parent)
        {
            output.Clear();
            string folder = parent.GetDirectoryPath();

            List<int> hashes = new List<int>();
            int length = archive.FileList.Count;
            int startIndex = 0;
            for (; startIndex < length; ++startIndex)
            {
                if (archive.FileList[startIndex].StartsWith(folder, StringComparison.OrdinalIgnoreCase))
                    break;
            }
            for (int i = startIndex; i < length; ++i)
            {
                string item = archive.FileList[i];
                int folderEndIndex = item.IndexOf(folder, 0, StringComparison.OrdinalIgnoreCase);
                if (folderEndIndex == -1)
                    return; // this is correct
                int nestedFolderIndex = item.IndexOf('\\', folderEndIndex + folder.Length + 1);
                if (nestedFolderIndex == -1)
                {
                    // this is file
                    output.Add(new ArchiveFilePath()
                    {
                        Type = FilePathType.File,
                        DisplayPath = item.Substring(folderEndIndex + folder.Length + 1),
                        RealPath = item,
                        Parent = parent
                    });
                }
                else
                {
                    // this is dir
                    var add = new ArchiveFilePath()
                    {
                        Type = FilePathType.Directory,
                        DisplayPath = item.Substring(folderEndIndex + folder.Length + 1, nestedFolderIndex - (folderEndIndex + folder.Length + 1)),
                        RealPath = item
                    };
                    int hash = StringComparer.OrdinalIgnoreCase.GetHashCode(add.DisplayPath);
                    if (hashes.Contains(hash))
                        continue;
                    hashes.Add(hash);
                    output.Add(add);
                }
            }
        }
        
        private static void SplitNames(string path)
        {
            m_names.Clear();

            int startIndex = 0;
            int length = 0;
            foreach (char c in path)
            {
                if (c == '\\')
                {
                    m_names.Add(path.Substring(startIndex, length));
                    startIndex += length + 1; // +1 to skip '\' char
                    length = 0;
                }
                else
                {
                    ++length;
                }
            }
            if (length != 0)
                m_names.Add(path.Substring(startIndex, length));
        }
    }
}
