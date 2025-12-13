namespace vatSysLauncher.Models
{
    public class PluginInstalled(string title, string profile, string localDirectory, Version currentVersion, Version localVersion, bool remove)
    {
        public string Title { get; set; } = title;
        public string Profile { get; set; } = profile;
        public string LocalDirectory { get; set; } = localDirectory;
        public Version LocalVersion { get; set; } = localVersion;
        public Version CurrentVersion { get; set; } = currentVersion;
        public bool UpdateAvailable
        {
            get
            {
                if (LocalVersion >= CurrentVersion) return false;
                return true;
            }
        }
        public bool Remove { get; set; } = remove;
        public string UpdateCommand => $"Update|Plugin|{Title}|{LocalDirectory}";
        public string DeleteCommand => $"Delete|Plugin|{Title}|{LocalDirectory}";
    }
}