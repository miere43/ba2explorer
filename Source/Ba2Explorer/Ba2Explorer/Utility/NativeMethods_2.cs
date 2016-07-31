using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Ba2Explorer.Utility
{
    internal enum WindowsOSVersion
    {
        NotWindows = -1,
        Unknown = 0,
        /// <summary>
        /// Windows Vista or later.
        /// </summary>
        Vista = 100,
    }

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

        internal static WindowsOSVersion GetWindowsVersion()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                return WindowsOSVersion.NotWindows;

            if (Environment.OSVersion.Version.Major >= 6)
                return WindowsOSVersion.Vista;

            return WindowsOSVersion.Unknown;
        }

        internal static bool IsWindowsVersionAtLeast(WindowsOSVersion version)
        {
            if (version == WindowsOSVersion.NotWindows || version == WindowsOSVersion.Unknown)
                return false;

            return (int)GetWindowsVersion() >= (int)version;
        }
    }
}
