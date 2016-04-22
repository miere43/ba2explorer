using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
