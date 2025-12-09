namespace vatSysManager
{
    public class ProfileOption(string title, string url, bool installed = false)
    {
        public string Title { get; set; } = title;
        public bool Installed { get; set; } = installed;
        public string LocalVersion { get; set; }
        public string CurrentVersion { get; set; }
        public string Url { get; set; } = url;
        public bool UpdateAvailable
        {
            get
            {
                if (string.IsNullOrWhiteSpace(CurrentVersion)) return false;
                if (string.IsNullOrWhiteSpace(LocalVersion)) return false;
                if (LocalVersion != CurrentVersion) return true;
                return false;
            }
        }
        public string DownloadUrl => $"{Url}/profile.zip";
        public string InstallCommand => $"Install|Profile|{Title}";
        public string UpdateCommand => $"Update|Profile|{Title}";
        public string DeleteCommand => $"Delete|Profile|{Title}";
    }
}
