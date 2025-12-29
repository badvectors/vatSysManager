using System.IO;
using System.Net.Http;
using System.Windows;
using vatSysLauncher.Models;
using vatSysLauncher.ViewModels;

namespace vatSysLauncher.Controllers
{
    public class Launcher
    {
        public static MainWindowViewModel MainViewModel { get; set; } = new MainWindowViewModel();
        public static readonly HttpClient HttpClient = new();
        public static Setting Settings = null;
        public static List<string> Changes = [];
        public static List<ProfileOption> ProfileOptions = [];
        public static List<PluginResponse> PluginsAvailable = [];
        public static List<PluginInstalled> PluginsInstalled = [];
        public static string CurrentCanvas = null;
        public static bool HasClearedCached = false;

        public static readonly string VatsysProcessName = "vatSys";
        public static string WorkingDirectory => $"{Settings.ProfileDirectory}\\Temp";
        public static string VatsysExe => $"{Settings.BaseDirectory}\\bin\\vatSys.exe";
        public static string PluginsBaseDirectory => $"{Settings.BaseDirectory}\\bin\\Plugins";
        public static string SettingsFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "vatSys Launcher");
        public static string RestartFile => Path.Combine(SettingsFolder, "Restart.txt");
        public static string SettingsFile => Path.Combine(SettingsFolder, "Settings.json");
        public static string UpdateFile => Path.Combine(SettingsFolder, "Update.txt");
        public static string PluginsFile => Path.Combine(SettingsFolder, "Plugins.json");
        public static string DefaultProfileDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "vatSys Files", "Profiles");
        public static string DefaultBaseDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "vatSys");

        public static string ProfilesUrl => "https://vatsys.sawbe.com/downloads/data/emptyprofiles/profiles.json";
        public static string PluginsUrl => "https://launcher.prod1.badvectors.dev/Plugins";
        public static string VersionUrl => "https://raw.githubusercontent.com/badvectors/vatSysLauncher/refs/heads/master/vatSysLauncher/LauncherVersion.json";
        public static string PluginsBaseDirectoryName => "All";

        public static void SetLoading(bool loading)
        {
            if (loading == true)
            {
                MainViewModel.ButtonsEnabled = false;
                MainViewModel.WaitText = Visibility.Visible;
                MainViewModel.LaunchButton = Visibility.Hidden;
                return;
            }

            MainViewModel.ButtonsEnabled = true;
            MainViewModel.WaitText = Visibility.Hidden;
            MainViewModel.LaunchButton = Visibility.Visible;
        }

        public static void SetCanvas(string canvasName)
        {
            MainViewModel.SetupCanvas = Visibility.Hidden;
            MainViewModel.InitCanvas = Visibility.Hidden;
            MainViewModel.HomeCanvas = Visibility.Hidden;
            MainViewModel.ProfilesCanvas = Visibility.Hidden;
            MainViewModel.UpdaterCanvas = Visibility.Hidden;
            MainViewModel.PluginsCanvas = Visibility.Hidden;

            CurrentCanvas = canvasName;

            switch (canvasName)
            {
                case "Setup":
                    MainViewModel.SetupCanvas = Visibility.Visible;
                    break;
                case "Init":
                    MainViewModel.InitCanvas = Visibility.Visible;
                    break;
                case "Home":
                    MainViewModel.HomeCanvas = Visibility.Visible;
                    break;
                case "Profiles":
                    MainViewModel.ProfilesCanvas = Visibility.Visible;
                    break;
                case "Updater":
                    MainViewModel.UpdaterCanvas = Visibility.Visible;
                    break;
                case "Plugins":
                    MainViewModel.PluginsCanvas = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }

        public static void GetChanges()
        {
            Changes.Clear();

            foreach (var plugin in PluginsInstalled)
            {
                if (!plugin.UpdateAvailable) continue;

                Changes.Add(plugin.UpdateCommand);
            }

            foreach (var plugin in PluginsInstalled)
            {
                if (!plugin.Remove) continue;

                Changes.Add(plugin.DeleteCommand);
            }

            foreach (var profile in ProfileOptions)
            {
                if (!profile.UpdateAvailable) continue;

                Changes.Add(profile.UpdateCommand);
            }

            if (Changes.Count == 0)
            {
                MainViewModel.UpdatesAvailable = Visibility.Hidden;
            }
            else
            {
                MainViewModel.UpdatesAvailable = Visibility.Visible;
                var updateText = "update";
                if (Changes.Count > 1) updateText = "updates";
                MainViewModel.UpdatesText = $"{Changes.Count} {updateText} to be installed.";
            }
        }

        public static async Task CheckForRestart()
        {
            if (!File.Exists(RestartFile)) return;

            var commands = await File.ReadAllLinesAsync(RestartFile);

            await Updater.Run(commands);

            File.Delete(RestartFile);
        }
    }
}