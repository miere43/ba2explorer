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
    internal enum StockIconIdentifier
    {
        /// <summary>
        /// Security shield. Use for UAC promts only.
        /// </summary>
        Shield = 77, 
    }

    internal static class StockIcon
    {
        internal static ImageSource Shield { get { return GetBitmapSource(StockIconIdentifier.Shield, 0); } }

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

        internal static BitmapImage ToBitmapImage(BitmapSource bitmapSource)
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

        private static ImageSource GetBitmapSource(StockIconIdentifier identifier, StockIconOptions flags)
        {
            ImageSource bitmapSource = MakeImage(identifier, StockIconOptions.Handle | flags);
            bitmapSource.Freeze();

            return bitmapSource;
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
    }
}
