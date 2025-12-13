using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Net.Http.Headers;
using System.Windows;
using vatSysLauncher.Models;
using vatSysManager;

namespace vatSysLauncher.Controllers
{
    public class Plugins
    {
        public static async Task Init()
        {
            Launcher.PluginsAvailable.Clear();

            Launcher.PluginsInstalled.Clear();

            Launcher.MainViewModel.PluginsLoading = Visibility.Visible;

            var available = await GetAvailable();

            Launcher.PluginsAvailable = available;

            var plugins = new List<string>();

            foreach (var plugin in available.Where(x => x.Remove == false))
            {
                plugins.Add(plugin.Name);
            }

            Launcher.MainViewModel.PluginsAvailable = plugins;

            Launcher.MainViewModel.PluginsLoading = Visibility.Hidden;

            var pluginOptions = new List<PluginInstalled>();

            var installed = GetInstalled();

            Launcher.PluginsInstalled = installed;

            Launcher.MainViewModel.PluginsList = installed;
        }

        private static async Task<List<PluginResponse>> GetAvailable()
        {
            var plugins = new List<PluginResponse>();

            var lastRefresh = await LastRefresh();

            if (lastRefresh > DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)))
            {
                return await GetSaved();
            }

            var response = await Launcher.HttpClient.GetAsync(Launcher.PluginsUrl);

            if (!response.IsSuccessStatusCode) return plugins;

            var content = await response.Content.ReadAsStringAsync();

            var pluginResponses = JsonConvert.DeserializeObject<List<PluginResponse>>(content);

            foreach (var pluginResponse in pluginResponses)
            {
                await GetVersion(pluginResponse);
            }

            plugins.AddRange(pluginResponses);

            await Save(plugins);

            return plugins;
        }

        private static List<PluginInstalled> GetInstalled()
        {
            var plugins = new List<PluginInstalled>();

            if (Launcher.Settings == null ||
                string.IsNullOrWhiteSpace(Launcher.Settings.BaseDirectory) ||
                string.IsNullOrWhiteSpace(Launcher.Settings.ProfileDirectory)) 
                return plugins;

            foreach (var directory in Directory.GetDirectories(Launcher.Settings.ProfileDirectory))
            {
                if (directory == Launcher.WorkingDirectory) continue;

                var profile = directory.Split('\\').Last();

                var subdirectories = Directory.GetDirectories(directory);

                var pluginDirectory = subdirectories.FirstOrDefault(x => x.EndsWith("Plugins"));

                if (pluginDirectory == null) continue;

                foreach (var dir in Directory.GetDirectories(pluginDirectory))
                {
                    var files = Directory.GetFiles(dir);

                    foreach (var file in files)
                    {
                        var split = file.Split('\\');

                        var pluginAvailable = Launcher.PluginsAvailable.FirstOrDefault(x => x.DllName == split.Last());

                        if (pluginAvailable == null) continue;

                        var localVersion = new Version();

                        try
                        {
                            var versionInfo = FileVersionInfo.GetVersionInfo(file);
                            localVersion = new Version(versionInfo.FileVersion);
                        }
                        catch { }

                        plugins.Add(new PluginInstalled(pluginAvailable.Name, profile, dir, pluginAvailable.Version, localVersion, pluginAvailable.Remove));

                        break;
                    }
                }
            }

            if (!Directory.Exists(Launcher.PluginsBaseDirectory)) return plugins;

            foreach (var dir in Directory.GetDirectories(Launcher.PluginsBaseDirectory))
            {
                var files = Directory.GetFiles(dir);

                foreach (var file in files)
                {
                    var split = file.Split('\\');

                    var pluginAvailable = Launcher.PluginsAvailable.FirstOrDefault(x => x.DllName == split.Last());

                    if (pluginAvailable == null) continue;

                    var localVersion = new Version();

                    try
                    {
                        var versionInfo = FileVersionInfo.GetVersionInfo(file);
                        localVersion = new Version(versionInfo.FileVersion);
                    }
                    catch { }

                    plugins.Add(new PluginInstalled(pluginAvailable.Name, Launcher.PluginsBaseDirectoryName, dir, pluginAvailable.Version, localVersion, pluginAvailable.Remove));

                    break;
                }
            }

            return plugins;
        }

        public static async Task<PluginResponse> GetVersion(PluginResponse pluginResponse)
        {
            if (pluginResponse == null) return null;

            if (pluginResponse.Remove == true) return null;

            try
            {
                Launcher.HttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("vatSysManager", "0.0.0"));

                Launcher.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));

                var latestPage = await Launcher.HttpClient.GetAsync(pluginResponse.LatestUrl);

                if (!latestPage.IsSuccessStatusCode) return pluginResponse;

                var latestPageContent = await latestPage.Content.ReadAsStringAsync();

                var gitHubResponse = JsonConvert.DeserializeObject<GitHubResponse>(latestPageContent);

                if (string.IsNullOrWhiteSpace(gitHubResponse.tag_name)) return null;

                var tagName = gitHubResponse.tag_name == "latest" ? gitHubResponse.name : gitHubResponse.tag_name;

                var version = new Version(0, 0, 0);

                try
                {
                    tagName = tagName.Replace("Version", "");
                    tagName = tagName.Replace("v", "");
                    tagName = tagName.Replace("-beta", "");
                    tagName = tagName.Replace("-pr", "");
                    tagName = tagName.Trim();
                    version = new Version(tagName);
                }
                catch { }

                pluginResponse.Version = version;

                if (!gitHubResponse.assets.Any()) return pluginResponse;

                pluginResponse.DownloadUrl = gitHubResponse.assets[0].browser_download_url;

                return pluginResponse;
            }
            catch
            {
                return pluginResponse;
            }
        }

        private static async Task<DateTime> LastRefresh()
        {
            if (!File.Exists(Launcher.UpdateFile)) return DateTime.MinValue;

            var lastUpdateText = await File.ReadAllTextAsync(Launcher.UpdateFile);

            var lastUpdateOk = DateTime.TryParse(lastUpdateText, out DateTime lastUpdate);

            if (!lastUpdateOk) return DateTime.MinValue;

            return lastUpdate;
        }

        public static async Task ClearCache()
        {
            if (Launcher.HasClearedCached) return;

            Launcher.HasClearedCached = true;

            Launcher.MainViewModel.ClearCacheButton = Visibility.Hidden;

            if (File.Exists(Launcher.UpdateFile)) File.Delete(Launcher.UpdateFile);

            await GetAvailable();
        }

        private static async Task<List<PluginResponse>> GetSaved()
        {
            var output = new List<PluginResponse>();

            if (!File.Exists(Launcher.PluginsFile)) return output;

            var pluginsText = await File.ReadAllTextAsync(Launcher.PluginsFile);

            try
            {
                output = JsonConvert.DeserializeObject<List<PluginResponse>>(pluginsText);
            }
            catch
            {
                return output;
            }

            return output;
        }

        private static async Task Save(List<PluginResponse> plugins)
        {
            if (File.Exists(Launcher.PluginsFile))
            {
                File.Delete(Launcher.PluginsFile);
            }

            if (File.Exists(Launcher.UpdateFile))
            {
                File.Delete(Launcher.UpdateFile);
            }

            var content = JsonConvert.SerializeObject(plugins);

            await File.WriteAllTextAsync(Launcher.PluginsFile, content);

            var lastUpdate = DateTime.UtcNow.ToString();

            await File.WriteAllTextAsync(Launcher.UpdateFile, lastUpdate);
        }
    }
}