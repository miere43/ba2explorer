using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ba2Explorer.Utility
{
    public static class IconCache
    {
        private static Dictionary<string, BitmapSource> m_cache = new Dictionary<string, BitmapSource>();

        public static ImageSource GetSmallIconFromExtension(string extension)
        {
            BitmapSource result;
            if (m_cache.TryGetValue(extension, out result))
                return result;
            result = NativeMethods.GetSmallIconFromExtension(extension);
            if (result == null)
                return null;
            m_cache[extension] = result;
            return result;
        }

        public static void DestroyCache()
        {
            // TODO
            //foreach (var icon in m_cache)
            //{
            //    DestroyIcon( /* get handle of icon */ )
            //}
        }
    }
}
