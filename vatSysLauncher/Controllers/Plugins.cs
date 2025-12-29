using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Windows;
using vatSysLauncher.Models;

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

            var response = await Launcher.HttpClient.GetAsync(Launcher.PluginsUrl);

            if (!response.IsSuccessStatusCode) return plugins;

            var content = await response.Content.ReadAsStringAsync();

            var pluginResponses = JsonConvert.DeserializeObject<List<PluginResponse>>(content);

            plugins.AddRange(pluginResponses);

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
    }
}