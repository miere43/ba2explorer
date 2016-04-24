using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Ba2Explorer.Utility
{
    internal partial class NativeMethods
    {
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        /// <summary>
        /// Changes an attribute of the specified window. The function also sets the 32-bit (long) value at the specified offset into the extra window memory.
        /// </summary>
        /// <param name="hWnd">A handle to the window and, indirectly, the class to which the window belongs..</param>
        /// <param name="nIndex">The zero-based offset to the value to be set. Valid values are in the range zero through the number of bytes of extra window memory, minus the size of an integer. To set any other value, specify one of the following values: GWL_EXSTYLE, GWL_HINSTANCE, GWL_ID, GWL_STYLE, GWL_USERDATA, GWL_WNDPROC </param>
        /// <param name="dwNewLong">The replacement value.</param>
        /// <returns>If the function succeeds, the return value is the previous value of the specified 32-bit integer. 
        /// If the function fails, the return value is zero. To get extended error information, call GetLastError. </returns>
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public static void ToggleCloseButton(IntPtr windowHandle)
        {
            SetWindowLong(windowHandle, GWL_STYLE, GetWindowLong(windowHandle, GWL_STYLE) & ~WS_SYSMENU);
        }

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct StockIconInfo
        {

            internal UInt32 StructureSize;

            internal IntPtr Handle;

            internal Int32 ImageIndex;

            internal Int32 Identifier;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]

            internal string Path;

        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false)]
        internal static extern int SHGetStockIconInfo(StockIconIdentifier identifier, StockIconOptions flags, ref StockIconInfo info);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool DestroyIcon(IntPtr handle);

        [Flags]
        internal enum StockIconOptions : uint
        {
            Small = 0x000000001,       // Retrieve the small version of the icon, as specified by the SM_CXSMICON and SM_CYSMICON system metrics.
            ShellSize = 0x000000004,   // Retrieve the shell-sized icons rather than the sizes specified by the system metrics.
            Handle = 0x000000100,      // The hIcon member of the SHSTOCKICONINFO structure receives a handle to the specified icon.
            SystemIndex = 0x000004000, // The iSysImageImage member of the SHSTOCKICONINFO structure receives the index of the specified icon in the system imagelist.
            LinkOverlay = 0x000008000, // Add the link overlay to the file’s icon.
            Selected = 0x000010000     // Blend the icon with the system highlight color.
        }


    }
}
