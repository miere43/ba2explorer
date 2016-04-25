using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

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
        OK = 0x0001,
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

    internal static class TaskDialog
    {
        [DllImport("comctl32.dll", PreserveSig = false, CharSet = CharSet.Unicode, EntryPoint = "TaskDialog")]
        private static extern TaskDialogResult ShowTaskDialog(IntPtr hwndParent, IntPtr hInstance, string title,
            string mainInstruction, string content, TaskDialogButtons buttons, TaskDialogIcon icon);

        /// <summary>
        /// Shows task dialog when running under Windows Vista and later, but falling back to normal message box when it's not available.
        /// </summary>
        /// <param name="owner">Owner window.</param>
        /// <param name="hInstance">hInstance. Use IntPtr.Zero.</param>
        /// <param name="title">Message box title.</param>
        /// <param name="header">Text that will be written with bigger font size.</param>
        /// <param name="message">Text that will be written with normal font size.</param>
        /// <param name="buttons">Buttons for dialog.</param>
        /// <param name="icon">Dialog icon.</param>
        /// <returns>Result.</returns>
        internal static TaskDialogResult Show(Window owner, IntPtr hInstance, string title,
            string header, string message, TaskDialogButtons buttons, TaskDialogIcon icon)
        {
            if (NativeMethods.IsWindowsVersionAtLeast(WindowsOSVersion.Vista))
            {
                // Play MessageBox sound.
                System.Media.SystemSounds.Exclamation.Play();
                return ShowTaskDialog(new WindowInteropHelper(owner).Handle, hInstance, title, header, message, buttons, icon);
            }
            else
                return ConvertMessageBoxResult(MessageBox.Show(owner, header + Environment.NewLine + Environment.NewLine + message,
                    title, ConvertTaskDialogButtons(buttons), ConvertTaskDialogIcon(icon)));
        }

        private static TaskDialogResult ConvertMessageBoxResult(MessageBoxResult result)
        {
            switch (result)
            {
                case MessageBoxResult.OK:
                    return TaskDialogResult.Ok;
                case MessageBoxResult.Yes:
                    return TaskDialogResult.Yes;
                case MessageBoxResult.No:
                    return TaskDialogResult.No;
                case MessageBoxResult.Cancel:
                case MessageBoxResult.None:
                    return TaskDialogResult.Cancel;
                default:
                    return TaskDialogResult.No;
            }
        }

        private static MessageBoxButton ConvertTaskDialogButtons(TaskDialogButtons buttons)
        {
            if (buttons.HasFlag(TaskDialogButtons.Yes) && buttons.HasFlag(TaskDialogButtons.No))
                if (buttons.HasFlag(TaskDialogButtons.Cancel))
                    return MessageBoxButton.YesNoCancel;
                else
                    return MessageBoxButton.YesNo;

            if (buttons.HasFlag(TaskDialogButtons.OK))
                if (buttons.HasFlag(TaskDialogButtons.Cancel))
                    return MessageBoxButton.OKCancel;
                else
                    return MessageBoxButton.OK;

            throw new NotSupportedException($"Cannot convert task dialog buttons { (int)buttons } to message box buttons.");
        }

        private static MessageBoxImage ConvertTaskDialogIcon(TaskDialogIcon icon)
        {
            switch (icon)
            {
                case TaskDialogIcon.Error:
                    return MessageBoxImage.Error;
                case TaskDialogIcon.Information:
                    return MessageBoxImage.Information;
                case TaskDialogIcon.Shield:
                    return MessageBoxImage.Question;
                case TaskDialogIcon.Warning:
                    return MessageBoxImage.Warning;
                default:
                    return MessageBoxImage.None;
            }
        }
    }
}
