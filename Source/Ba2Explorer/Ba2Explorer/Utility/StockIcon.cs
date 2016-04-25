using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static Ba2Explorer.Utility.NativeMethods;

namespace Ba2Explorer.Utility
{
    internal static class StockIcon
    {
        /// <summary>
        /// Shield UAC icon.
        /// </summary>
        internal static ImageSource Shield { get { return GetImageSource(StockIconIdentifier.Shield, 0); } }

        #region Private

        private enum StockIconIdentifier
        {
            /// <summary>
            /// Security shield. Use for UAC promts only.
            /// </summary>
            Shield = 77,
        }

        private static ImageSource MakeImage(StockIconIdentifier identifier, StockIconOptions flags)
        {
            IntPtr iconHandle = GetIcon(identifier, flags);
            ImageSource imageSource;

            try
            {
                imageSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(iconHandle, Int32Rect.Empty, null);
            }
            finally
            {
                DestroyIcon(iconHandle);
            }

            return imageSource;
        }

        private static BitmapImage ToBitmapImage(BitmapSource bitmapSource)
        {
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            MemoryStream memorystream = new MemoryStream();
            BitmapImage tmpImage = new BitmapImage();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            encoder.Save(memorystream);

            tmpImage.BeginInit();
            tmpImage.StreamSource = new MemoryStream(memorystream.ToArray());
            tmpImage.EndInit();

            memorystream.Close();
            return tmpImage;
        }

        private static ImageSource GetImageSource(StockIconIdentifier identifier, StockIconOptions flags)
        {
            ImageSource imageSource = MakeImage(identifier, StockIconOptions.Handle | flags);
            imageSource.Freeze();

            return imageSource;
        }

        private static IntPtr GetIcon(StockIconIdentifier identifier, StockIconOptions flags)
        {
            StockIconInfo info = new StockIconInfo();
            info.StructureSize = (UInt32)Marshal.SizeOf(typeof(StockIconInfo));

            int hResult = SHGetStockIconInfo(identifier, flags, ref info);

            if (hResult < 0)
                throw new COMException("SHGetStockIconInfo", hResult);

            return info.Handle;
        }

        #endregion

        #region P/Invoke Imports

        [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct StockIconInfo
        {
            internal UInt32 StructureSize;

            internal IntPtr Handle;

            internal Int32 ImageIndex;

            internal Int32 Identifier;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            internal string Path;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false)]
        private static extern int SHGetStockIconInfo(StockIconIdentifier identifier, StockIconOptions flags, ref StockIconInfo info);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr handle);

        [Flags]
        private enum StockIconOptions : uint
        {
            Small = 0x000000001,       // Retrieve the small version of the icon, as specified by the SM_CXSMICON and SM_CYSMICON system metrics.
            ShellSize = 0x000000004,   // Retrieve the shell-sized icons rather than the sizes specified by the system metrics.
            Handle = 0x000000100,      // The hIcon member of the SHSTOCKICONINFO structure receives a handle to the specified icon.
            SystemIndex = 0x000004000, // The iSysImageImage member of the SHSTOCKICONINFO structure receives the index of the specified icon in the system imagelist.
            LinkOverlay = 0x000008000, // Add the link overlay to the file’s icon.
            Selected = 0x000010000     // Blend the icon with the system highlight color.
        }

        #endregion
    }
}
