namespace vatSysLauncher.Models
{
    public class PluginResponse
    {
        public string Name { get; set; }
        public string DllName { get; set; }
        public bool Remove { get; set; }
        public bool Development { get; set; }
        public string LatestUrl => $"https://api.github.com/repos/{Name}/releases/latest";
        public string DirectoryName => Name.Split('/').Last();
        public Version Version { get; set; }
        public string DownloadUrl { get; set; }
    }
}