namespace Ba2Explorer.Settings
{
    internal class GlobalSettings : AppSettingsBase
    {
        public int Version { get; set; } = -1;

        public bool IsFirstLaunch { get; set; } = true;

        public string Culture { get; set; } = "en-US";

        public override void Saving()
        {
            IsFirstLaunch = false;

            base.Saving();
        }
    }
}
