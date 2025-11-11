using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace vatSysManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly string VatsysProcessName = "vatSys";
        private static readonly DispatcherTimer VatSysTimer = new();
        private static Canvas CurrentCanvas = null;
        private static Settings Settings = null;
        private static HttpClient HttpClient = new();
        private static List<ProfileOption> ProfileOptions = [];

        private static string WorkingDirectory => $"{Settings.ProfileDirectory}\\Temp";

        public MainWindow()
        {
            InitializeComponent();
            _ = Init();
        }

        private async Task Init()
        {
            InitSettings();

            HomeButton_Click(null, null);

            HomeButton.IsEnabled = false;
            PluginsButton.IsEnabled = false;
            ProfilesButton.IsEnabled = false;
            SetupButton.IsEnabled = false;
            WaitTextBlock.Visibility = Visibility.Visible;
            LaunchButton.Visibility = Visibility.Hidden;

            await InitProfiles();

            HomeButton.IsEnabled = true;
            //PluginsButton.IsEnabled = true;
            ProfilesButton.IsEnabled = true;
            SetupButton.IsEnabled = true;
            WaitTextBlock.Visibility = Visibility.Hidden;
            LaunchButton.Visibility = Visibility.Visible;

            VatSysCheck();

            VatSysTimer.Tick += VatSysTimer_Tick;
            VatSysTimer.Interval = new TimeSpan(0, 0, 1);

            VatSysTimer.Start();
        }

        private async Task InitProfiles()
        {
            var profiles = new List<ProfileOption>();

            var installed = ProfilesGetInstalled();

            profiles.AddRange(installed);

            var available = await ProfilesGetAvailable();

            foreach (var profile in available)
            {
                var existing = profiles.FirstOrDefault(x => x.Title == profile.Title);
                if (existing != null)
                {
                    existing.Url = profile.Url;
                    existing.CurrentVersion = profile.CurrentVersion;
                    continue;
                }
                profiles.Add(profile);
            }

            ProfileOptions = profiles;
        }

        private void InitSettings()
        {
            var settings = new Settings();

            var defaultProfileDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "vatSys Files", "Profiles");
            
            var defaultBaseDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "vatSys");

            if (Directory.Exists(defaultProfileDirectory))
            {
                settings.ProfileDirectory = defaultProfileDirectory;
            }

            if (Directory.Exists(defaultBaseDirectory))
            {
                settings.BaseDirectory = defaultBaseDirectory;
            }

            Settings = settings;
        }

        private void VatSysTimer_Tick(object sender, EventArgs e)
        {
            VatSysCheck();
        }

        private void VatSysCheck()
        {
            // Get any running vatSys processes.
            var vatsysProcesses = Process.GetProcessesByName(VatsysProcessName);

            if (vatsysProcesses.Length > 0)
            {
                InitCheckCanvas.Visibility = Visibility.Visible;
                HomeButton.IsEnabled = false;
                HomeCanvas.Visibility = Visibility.Hidden;
                PluginsButton.IsEnabled = false;
                ProfilesButton.IsEnabled = false;
                SetupButton.IsEnabled = false;
                SetupCanvas.Visibility = Visibility.Hidden;
                ProfilesCanvas.Visibility = Visibility.Hidden;
                UpdaterCanvas.Visibility = Visibility.Hidden;
            }
            else
            {
                InitCheckCanvas.Visibility = Visibility.Hidden;
                HomeButton.IsEnabled = true;
                ///PluginsButton.IsEnabled = true;
                ProfilesButton.IsEnabled = true;
                SetupButton.IsEnabled = true;

                if (CurrentCanvas == null) HomeCanvas.Visibility = Visibility.Visible;
                else CurrentCanvas.Visibility = Visibility.Visible;
            }
        }

        private void VatSysClose()
        {
            // Get any running vatSys processes.
            var vatsysProcesses = Process.GetProcessesByName(VatsysProcessName);

            // Kill all running vatSys processes.
            if (vatsysProcesses.Length > 0)
            {
                foreach (var vatsysProcess in vatsysProcesses)
                    vatsysProcess.Kill();
            }
        }

        private void VatSysCloseButton_Click(object sender, RoutedEventArgs e)
        {
            VatSysClose();
        }

        private void VatSysLaunchButton_Click(object sender, RoutedEventArgs e)
        {
            if (Settings == null || string.IsNullOrWhiteSpace(Settings.BaseDirectory)) return;
            Process.Start($"{Settings.BaseDirectory}\\bin\\vatSys.exe");
            Environment.Exit(1);
        }

        private void SetupButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentCanvas = SetupCanvas;
            SetupCanvas.Visibility = Visibility.Visible;
            InitCheckCanvas.Visibility = Visibility.Hidden;
            HomeCanvas.Visibility = Visibility.Hidden;
            ProfilesCanvas.Visibility = Visibility.Hidden;
            UpdaterCanvas.Visibility = Visibility.Hidden;

            if (Settings == null) return;
            BaseDirectoryTextBox.Text = Settings.BaseDirectory;
            ProfileDirectoryTextBox.Text = Settings.ProfileDirectory;
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentCanvas = HomeCanvas;
            SetupCanvas.Visibility = Visibility.Hidden;
            InitCheckCanvas.Visibility = Visibility.Hidden;
            HomeCanvas.Visibility = Visibility.Visible;
            ProfilesCanvas.Visibility = Visibility.Hidden;
            UpdaterCanvas.Visibility = Visibility.Hidden;
        }

        private void ProfilesButton_Click(object sender, RoutedEventArgs e)
        {
            ProfilesList.ItemsSource = ProfileOptions;

            CurrentCanvas = ProfilesCanvas;
            SetupCanvas.Visibility = Visibility.Hidden;
            InitCheckCanvas.Visibility = Visibility.Hidden;
            HomeCanvas.Visibility = Visibility.Hidden;
            ProfilesCanvas.Visibility = Visibility.Visible;
            UpdaterCanvas.Visibility = Visibility.Hidden;
        }

        private static async Task<List<ProfileOption>> ProfilesGetAvailable()
        {
            var profiles = new List<ProfileOption>();

            var url = "https://vatsys.sawbe.com/downloads/data/emptyprofiles/profiles.json";

            var response = await HttpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) return profiles;

            var responseString = await response.Content.ReadAsStringAsync();

            var available = JsonConvert.DeserializeObject<List<ProfilesResponse>>(responseString);

            foreach (var profile in available)
            {
                var profileOption = new ProfileOption(profile.name, profile.path);

                var profileFile = $"{profile.path}/Profile.xml";

                var profileResponse = await HttpClient.GetAsync(profileFile);

                var contents = await profileResponse.Content.ReadAsStringAsync();

                profileOption.CurrentVersion = ProfileGetVersion(contents);

                profiles.Add(profileOption);
            }

            return profiles;
        }

        private static List<ProfileOption> ProfilesGetInstalled()
        {
            var profiles = new List<ProfileOption>();

            if (Settings == null || string.IsNullOrWhiteSpace(Settings.ProfileDirectory)) return profiles;

            foreach (var directory in Directory.GetDirectories(Settings.ProfileDirectory))
            {
                if (directory == WorkingDirectory) continue;

                var profileOption = new ProfileOption(directory.Split('\\').Last(), null, true);

                var profileFile = Path.Combine(directory, "Profile.xml");

                if (File.Exists(profileFile))
                {
                    var contents = File.ReadAllText(profileFile);

                    profileOption.LocalVersion = ProfileGetVersion(contents);
                }

                profiles.Add(profileOption);
            }

            return profiles;
        }

        private static string ProfileGetVersion(string contents)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(Profile));

                using var reader = new StringReader(contents);

                var profileXml = (Profile)serializer.Deserialize(reader);

                if (!string.IsNullOrWhiteSpace(profileXml.Version.Revision))
                {
                    return $"{profileXml.Version.AIRAC}.{profileXml.Version.Revision}";
                }

                return $"{profileXml.Version.AIRAC}";
            }
            catch
            {
                return "ERROR";
            }
        }

        private void UpdaterCanvasMode()
        {
            CurrentCanvas = UpdaterCanvas;
            SetupCanvas.Visibility = Visibility.Hidden;
            InitCheckCanvas.Visibility = Visibility.Hidden;
            HomeCanvas.Visibility = Visibility.Hidden;
            ProfilesCanvas.Visibility = Visibility.Hidden;
            UpdaterCanvas.Visibility = Visibility.Visible;

            UpdaterLog.Text = string.Empty;
        }

        private static void SetAttributesNormal(DirectoryInfo dir)
        {
            foreach (var subDir in dir.GetDirectories())
            {
                SetAttributesNormal(subDir);

                subDir.Attributes = FileAttributes.Normal;
            }
            foreach (var file in dir.GetFiles())
            {
                file.Attributes = FileAttributes.Normal;
            }
            dir.Attributes = FileAttributes.Normal;
        }

        private void UpdaterButton_Click(object sender, RoutedEventArgs e)
        {
            UpdaterCanvasMode();

            UpdaterAction(((Button)sender).Tag.ToString());
        }

        private async void UpdaterAction(string code)
        {
            var split = code.Split(':');

            if (split[0] == "Delete")
            {
                if (split[1] == "Profile")
                {
                    // delete directory

                    var directory = Path.Combine(Settings.ProfileDirectory, split[2]);

                    var success = RunProfileDelete(directory);

                    if (!success) return;

                    // if success return to profile screen

                    await InitProfiles();

                    ProfilesButton_Click(null, null);
                }
            }
            else if (split[0] == "Install")
            {
                if (split[1] == "Profile")
                {
                    var profileOption = ProfileOptions.FirstOrDefault(x => x.Title == split[2]);

                    if (profileOption == null) return;

                    var directory = Path.Combine(Settings.ProfileDirectory, split[2]);

                    if (Path.Exists(directory)) return;

                    var success = await RunProfileInstall(profileOption);

                    if (!success) return;

                    // if success return to profile screen

                    await InitProfiles();

                    ProfilesButton_Click(null, null);
                }
            }
            else if (split[0] == "Update")
            {
                if (split[1] == "Profile")
                {
                    var profileOption = ProfileOptions.FirstOrDefault(x => x.Title == split[2]);

                    if (profileOption == null) return;

                    var directory = Path.Combine(Settings.ProfileDirectory, split[2]);

                    if (!Path.Exists(directory)) return;

                    var success = RunProfileDelete(directory);

                    if (!success) return;

                    success = await RunProfileInstall(profileOption);

                    if (!success) return;

                    // if success return to profile screen

                    await InitProfiles();

                    ProfilesButton_Click(null, null);
                }
            }
        }



        private bool RunProfileDelete(string directory)
        {
            // delete directory

            var result = DeleteDirectory(directory);

            UpdaterOutput(result);

            if (!result.Success) return false;

            return true;
        }

        private async Task<bool> RunProfileInstall(ProfileOption profileOption)
        {
            // create working directory

            var workingResult = CreateDirectory(WorkingDirectory);

            UpdaterOutput(workingResult);

            if (!workingResult.Success) return false;

            // download plugin

            var downloadResult = await DownloadProfile(profileOption.DownloadUrl);

            UpdaterOutput(downloadResult);

            if (!downloadResult.Success) return false;

            // create directory

            var directoryResult = CreateDirectory(Path.Combine(Settings.ProfileDirectory, profileOption.Title));

            UpdaterOutput(directoryResult);

            if (!directoryResult.Success) return false;

            // extract profile

            var extractResult = Extract(Path.Combine(WorkingDirectory, "Plugin.zip"), Path.Combine(Settings.ProfileDirectory, profileOption.Title));

            UpdaterOutput(extractResult);

            if (!extractResult.Success) return false;

            // delete working directory

            var deleteResult = DeleteDirectory(WorkingDirectory);

            UpdaterOutput(deleteResult);

            if (!deleteResult.Success) return false;

            return true;
        }


        private void UpdaterOutput(UpdaterResult result)
        {
            foreach (var item in result.Log)
            {
                UpdaterLog.Text += item + Environment.NewLine;
            }
        }

        public static UpdaterResult CreateDirectory(string directory)
        {
            var result = new UpdaterResult();

            if (!Directory.Exists(directory))
            {
                result.Log.Add($"Creating directory: {directory}.");

                try
                {
                    Directory.CreateDirectory(directory);
                }
                catch (Exception ex)
                {
                    result.Log.Add($"Could not create directory: {ex.Message}");

                    return result;
                }
            }

            result.Success = true;

            return result;
        }

        public static UpdaterResult DeleteDirectory(string directory)
        {
            var result = new UpdaterResult();

            if (Directory.Exists(directory))
            {
                result.Log.Add($"Deleting directory: {directory}.");

                try
                {
                    DirectoryInfo dir = new(directory);

                    SetAttributesNormal(dir);

                    dir.Delete(true);
                }
                catch (Exception ex)
                {
                    result.Log.Add($"Could not delete directory: {ex.Message}");

                    return result;
                }
            }

            result.Success = true;

            return result;
        }

        public static UpdaterResult Extract(string zipFile, string toDirectory)
        {
            var result = new UpdaterResult();

            result.Log.Add("Extracting plugin.");

            try
            {
                ZipFile.ExtractToDirectory(zipFile, toDirectory);
            }
            catch (Exception ex)
            {
                result.Log.Add($"Could not extract plugin: {ex.Message}");

                if (ex.InnerException != null)
                {
                    result.Log.Add($"-> {ex.InnerException.Message}");
                }

                return result;
            }

            string[] fileEntries = Directory.GetFiles(toDirectory);

            foreach (var file in fileEntries)
            {
                System.IO.File.SetAttributes(file, FileAttributes.Normal);
            }

            result.Log.Add("Extract completed.");

            result.Success = true;

            return result;
        }

        public static async Task<UpdaterResult> DownloadProfile(string url)
        {
            var result = new UpdaterResult();

            result.Log.Add($"Downloading from: {url}.");

            using (var downloadResponse = await HttpClient.GetAsync(url))
            {
                if (!downloadResponse.IsSuccessStatusCode)
                {
                    result.Log.Add($"Could not download plugin: {downloadResponse.StatusCode}.");

                    return result;
                }

                using (var stream = await downloadResponse.Content.ReadAsStreamAsync())
                using (var file = System.IO.File.OpenWrite(System.IO.Path.Combine(WorkingDirectory, "Plugin.zip")))
                {
                    stream.CopyTo(file);
                }
            }

            result.Log.Add("Download completed.");

            result.Success = true;

            return result;
        }

        public class UpdaterResult
        {
            public bool Success { get; set; } = false;
            public List<string> Log { get; set; } = [];
        }
    }
}