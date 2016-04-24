using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace Ba2Explorer.Utility
{
    internal static class OpenAppUtil
    {
        /// <summary>
        /// Safely opens file using associated application. Tries to not
        /// execute .exe, .bat and similar files.
        /// </summary>
        /// <param name="filePath"></param>
        internal static void RunFileSafe(string filePath)
        {
            string ext = Path.GetExtension(filePath);

            if (ext == ".bat" || ext == ".ps1"
             || ext == ".exe" || ext == ".cmd")
            {
                return;
            }

            if (!File.Exists(filePath))
                return;

            Process.Start(filePath);
        }

        /// <summary>
        /// Opens Explorer, which opens directory or selecting the file passed in <c>path</c> argument.
        /// </summary>
        internal static void ExplorerOpenPath(Window ownerWindow, string path, bool isFile)
        {
            ProcessStartInfo info = new ProcessStartInfo();
            info.ErrorDialog = true;
            info.ErrorDialogParentHandle = new WindowInteropHelper(ownerWindow).Handle;
            info.FileName = "explorer";
            info.UseShellExecute = true;

            bool failed = false;

            if (isFile)
            {
                if (File.Exists(path))
                {
                    info.Arguments = @"/select," + path;
                }
                else if (Directory.Exists(Path.GetDirectoryName(path)))
                {
                    info.Arguments = Path.GetDirectoryName(path);
                }
                else
                {
                    failed = true;
                }
            }
            else
            {
                if (Directory.Exists(path))
                {
                    info.Arguments = path;
                }
                else
                {
                    failed = true;
                }
            }

            if (failed)
            {
                MessageBox.Show(ownerWindow, "Cannot open file folder: file nor folder are exist", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Warning, MessageBoxResult.OK, MessageBoxOptions.None);
                return;
            }

            var process = Process.Start(info);
            process.Dispose();
        }


    }
}
