using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ba2Explorer.Utility;
using System.Windows;
using System.Security;
using Microsoft.Win32;

namespace Ba2Explorer
{
    internal static class ExtensionAssociation
    {
        private const string associateExtension = ".ba2";
        private const string associateKeyName = "BA2Explorer.ba2";
        private const string associateFriendlyName = "Bethesda Archive 2";
        static readonly string associateExePath = System.Reflection.Assembly.GetExecutingAssembly().Location;

        static readonly string promptInstructions = "Pressing OK will give you a prompt to restart BA2 Explorer with admin rights, " +
            "so it can update extensions registry. " + Environment.NewLine + Environment.NewLine + "Press Cancel to abort.";
        private const string promptTitle = "Administrator rights required";
        private const string propmtHeader = "Admin rights required to (un)associate archive extension from BA2 Explorer";

        /// <summary>
        /// Returns true if extension is associated.
        /// </summary>
        public static bool IsExtensionAssociated => Registry.ClassesRoot.OpenSubKey(associateKeyName) != null;

        public static bool CanAssociateExtension => UACElevationHelper.IsRunAsAdmin() && UACElevationHelper.IsProcessElevated();

        private static bool UnassociateBA2Extension()
        {
            try
            {
                using (var baseKey = Registry.ClassesRoot.OpenSubKey(associateExtension, true))
                {
                    baseKey.DeleteValue("", false);
                }

                Registry.ClassesRoot.DeleteSubKeyTree(associateKeyName, false);

                // tell explorer to update icons
                NativeMethods.SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);

                return true;
            }
            catch (SecurityException)
            {
                return false;
            }
        }

        private static bool AssociateBA2Extension()
        {
            try
            {
                RegistryKey BaseKey;
                RegistryKey OpenMethod;
                RegistryKey Shell;

                BaseKey = Registry.ClassesRoot.CreateSubKey(associateExtension);
                BaseKey.SetValue("", associateKeyName);

                OpenMethod = Registry.ClassesRoot.CreateSubKey(associateKeyName);
                OpenMethod.SetValue("", associateFriendlyName);
                OpenMethod.CreateSubKey("DefaultIcon").SetValue("", "\"" + associateExePath + "\",0");
                Shell = OpenMethod.CreateSubKey("Shell");
                Shell.CreateSubKey("edit").CreateSubKey("command").SetValue("", "\"" + associateExePath + "\"" + " \"%1\"");
                Shell.CreateSubKey("open").CreateSubKey("command").SetValue("", "\"" + associateExePath + "\"" + " \"%1\"");
                BaseKey.Close();
                OpenMethod.Close();
                Shell.Close();

                // Tell explorer the file association has been changed
                NativeMethods.SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);

                return true;
            }
            catch (SecurityException)
            {
                return false;
            }
        }

        /// <summary>
        /// same as MessageBox.Show, but doesn't crash when owner == null.
        /// </summary>
        private static MessageBoxResult ShowMessageBox(Window owner, string text, string title, MessageBoxButton buttons,
            MessageBoxImage image)
        {
            if (owner == null)
                return MessageBox.Show(text, title, buttons, image);
            else
                return MessageBox.Show(owner, text, title, buttons, image);
        }

        private static bool ShowAssociationStatusMessage(Window parentWindow)
        {
            bool isAssoc = IsExtensionAssociated;
            if (isAssoc)
            {
                ShowMessageBox(parentWindow, "Successfully associated extension.", "Success", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                LogAssociationError();
                ShowMessageBox(parentWindow, "Error occured while associating extension.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            return isAssoc;
        }

        private static bool ShowUnassociationStatusMessage(Window parentWindow)
        {
            bool isAssoc = IsExtensionAssociated;
            if (!isAssoc)
            {
                ShowMessageBox(parentWindow, "Successfully unassociated extension.", "Success", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                LogAssociationError();
                ShowMessageBox(parentWindow, "Error occured while unassociating extension.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            return !isAssoc;
        }

        public static bool TryAssociate(Window parentWindow, bool alreadyTriedToAssociate)
        {
            if (CanAssociateExtension || alreadyTriedToAssociate)
            {
                AssociateBA2Extension();
                return ShowAssociationStatusMessage(parentWindow);
            }
            else
            {
                var result = TaskDialog.Show(parentWindow, IntPtr.Zero, promptTitle, propmtHeader,
                       promptInstructions, TaskDialogButtons.OK | TaskDialogButtons.Cancel, TaskDialogIcon.Shield);

                if (result == TaskDialogResult.Ok)
                {
                    UACElevationHelper.Elevate(@"/associate-extension");
                }
            }

            return false;
        }

        public static bool TryUnassociate(Window parentWindow, bool alreadyTriedToUnassociate)
        {
            if (CanAssociateExtension || alreadyTriedToUnassociate)
            {
                UnassociateBA2Extension();
                return ShowUnassociationStatusMessage(parentWindow);
            }
            else
            {
                var result = TaskDialog.Show(parentWindow, IntPtr.Zero, promptTitle, propmtHeader,
                       promptInstructions, TaskDialogButtons.OK | TaskDialogButtons.Cancel, TaskDialogIcon.Shield);

                if (result == TaskDialogResult.Ok)
                {
                    UACElevationHelper.Elevate(@"/unassociate-extension");
                }
            }
            return false;
        }

        private static void LogAssociationError()
        {
            App.Logger.Log(Logging.LogPriority.Error, "Error while (un)associating extension (is admin: {0}, elevated: {1}, " +
                "is in admin group: {2}, integrity level: {3})", UACElevationHelper.IsRunAsAdmin(),
                UACElevationHelper.IsProcessElevated(), UACElevationHelper.IsUserInAdminGroup(),
                UACElevationHelper.GetProcessIntegrityLevel());
        }
    }
}
