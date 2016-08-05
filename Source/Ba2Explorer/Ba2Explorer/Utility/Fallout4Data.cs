using System;
using Microsoft.Win32;

namespace Ba2Explorer.Utility
{
    public static class Fallout4Data
    {
        /// <summary>
        /// Rethieves Fallout 4 installation path from registry.
        /// </summary>
        /// <returns>Fallout 4 installation path or null if not found.</returns>
        public static string GetFallout4InstallationPath()
        {
            var hklm = Registry.LocalMachine;
            if (hklm == null) return null;
            var hkSoftware = hklm.OpenSubKey("SOFTWARE");
            if (hkSoftware == null) return null;

            RegistryKey hkEnvNode;
            if (Environment.Is64BitOperatingSystem)
                hkEnvNode = hkSoftware.OpenSubKey("WOW6432Node");
            else
                hkEnvNode = hkSoftware;
            if (hkEnvNode == null) return null;

            var hkBethesdaSoftworks = hkEnvNode.OpenSubKey("bethesda softworks");
            if (hkBethesdaSoftworks == null) return null;

            var hkFallout4 = hkBethesdaSoftworks.OpenSubKey("Fallout4");
            if (hkFallout4 == null) return null;

            object installedPath = hkFallout4.GetValue("Installed Path");
            return installedPath as string;
        }
    }
}
