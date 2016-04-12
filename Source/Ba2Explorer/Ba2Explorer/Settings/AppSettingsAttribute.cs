using System;

namespace Ba2Explorer.Settings
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    internal sealed class AppSettingsAttribute : Attribute
    {
        public readonly string TomlSection;

        public AppSettingsAttribute(string tomlSection)
        {
            this.TomlSection = tomlSection;
        }
    }
}
