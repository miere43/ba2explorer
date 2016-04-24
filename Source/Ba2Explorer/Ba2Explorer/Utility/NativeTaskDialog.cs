using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Ba2Explorer.Utility
{
    internal enum TaskDialogResult
    {
        Ok = 1,
        Cancel = 2,
        Retry = 4,
        Yes = 6,
        No = 7,
        Close = 8
    }

    [Flags]
    internal enum TaskDialogButtons
    {
        Ok = 0x0001,
        Yes = 0x0002,
        No = 0x0004,
        Cancel = 0x0008,
        Retry = 0x0010,
        Close = 0x0020
    }

    internal enum TaskDialogIcon
    {
        Warning = 65535,
        Error = 65534,
        Information = 65533,
        Shield = 65532
    }

    internal static class NativeTaskDialog
    {
        [DllImport("comctl32.dll", PreserveSig = false, CharSet = CharSet.Unicode, EntryPoint = "TaskDialog")]
        internal static extern TaskDialogResult Show(IntPtr hwndParent, IntPtr hInstance, string title,
            string mainInstruction, string content, TaskDialogButtons buttons, TaskDialogIcon icon);
    }
}
