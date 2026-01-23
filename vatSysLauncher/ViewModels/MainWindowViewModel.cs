using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using vatSysLauncher.Controllers;
using vatSysLauncher.Models;

namespace vatSysLauncher.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        // Main

        private bool _buttonsEnabled;
        private Visibility _waitText;
        private Visibility _launchButton;
        private Visibility _updatesAvailable;
        private string _updatesText;

        public string Version => $"Version {Utility.GetFileVersion()}";
        public bool ButtonsEnabled
        {
            get { return _buttonsEnabled; }
            set
            {
                if (_buttonsEnabled != value)
                {
                    _buttonsEnabled = value;
                    OnPropertyChanged();
                }
            }
        }
        public Visibility WaitText
        {
            get { return _waitText; }
            set
            {
                if (_waitText != value)
                {
                    _waitText = value;
                    OnPropertyChanged();
                }
            }
        }
        public Visibility LaunchButton
        {
            get { return _launchButton; }
            set
            {
                if (_launchButton != value)
                {
                    _launchButton = value;
                    OnPropertyChanged();
                }
            }
        }
        public Visibility UpdatesAvailable
        {
            get { return _updatesAvailable; }
            set
            {
                if (_updatesAvailable != value)
                {
                    _updatesAvailable = value;
                    OnPropertyChanged();
                }
            }
        }
        public string UpdatesText
        {
            get { return _updatesText; }
            set
            {
                if (_updatesText != value)
                {
                    _updatesText = value;
                    OnPropertyChanged();
                }
            }
        }

        // Init

        private Visibility _initCanvas;

        public Visibility InitCanvas
        {
            get { return _initCanvas; }
            set
            {
                if (_initCanvas != value)
                {
                    _initCanvas = value;
                    OnPropertyChanged();
                }
            }
        }

        // Home

        private Visibility _homeCanvas;

        public Visibility HomeCanvas
        {
            get { return _homeCanvas; }
            set
            {
                if (_homeCanvas != value)
                {
                    _homeCanvas = value;
                    OnPropertyChanged();
                }
            }
        }

        // Profiles

        private Visibility _profilesCanvas;
        private Visibility _profilesLoading;
        private List<ProfileOption> _profilesList = new();

        public Visibility ProfilesCanvas
        {
            get { return _profilesCanvas; }
            set
            {
                if (_profilesCanvas != value)
                {
                    _profilesCanvas = value;
                    OnPropertyChanged();
                }
            }
        }
        public Visibility ProfilesLoading
        {
            get { return _profilesLoading; }
            set
            {
                if (_profilesLoading != value)
                {
                    _profilesLoading = value;
                    OnPropertyChanged();
                }
            }
        }
        public List<ProfileOption> ProfilesList
        {
            get { return _profilesList; }
            set
            {
                if (_profilesList != value)
                {
                    _profilesList = value;
                    OnPropertyChanged();
                }
            }
        }

        // Plugins

        private Visibility _pluginsCanvas;
        private Visibility _pluginsLoading;
        private List<PluginInstalled> _pluginsList;
        private List<string> _pluginsLocations = new();
        private List<string> _pluginsAvailable = new();

        public Visibility PluginsCanvas
        {
            get { return _pluginsCanvas; }
            set
            {
                if (_pluginsCanvas != value)
                {
                    _pluginsCanvas = value;
                    OnPropertyChanged();
                }
            }
        }
        public Visibility PluginsLoading
        {
            get { return _pluginsLoading; }
            set
            {
                if (_pluginsLoading != value)
                {
                    _pluginsLoading = value;
                    OnPropertyChanged();
                }
            }
        }
        public List<PluginInstalled> PluginsList
        {
            get { return _pluginsList; }
            set
            {
                if (_pluginsList != value)
                {
                    _pluginsList = value;
                    OnPropertyChanged();
                }
            }
        }
        public List<string> PluginsLocations
        {
            get { return _pluginsLocations; }
            set
            {
                if (_pluginsLocations != value)
                {
                    _pluginsLocations = value;
                    OnPropertyChanged();
                }
            }
        }
        public List<string> PluginsAvailable
        {
            get { return _pluginsAvailable; }
            set
            {
                if (_pluginsAvailable != value)
                {
                    _pluginsAvailable = value;
                    OnPropertyChanged();
                }
            }
        }

        // Updater

        private Visibility _updaterCanvas;
        private string _updaterLog;

        public Visibility UpdaterCanvas
        {
            get { return _updaterCanvas; }
            set
            {
                if (_updaterCanvas != value)
                {
                    _updaterCanvas = value;
                    OnPropertyChanged();
                }
            }
        }
        public string UpdaterLog
        {
            get { return _updaterLog; }
            set
            {
                if (_updaterLog != value)
                {
                    _updaterLog = value;
                    OnPropertyChanged();
                }
            }
        }

        // Setup

        private Visibility _setupCanvas;

        public Visibility SetupCanvas
        {
            get { return _setupCanvas; }
            set
            {
                if (_setupCanvas != value)
                {
                    _setupCanvas = value;
                    OnPropertyChanged();
                }
            }
        }
        public string BaseDirectory => Launcher.Settings.BaseDirectory;
        public string ProfileDirectory => Launcher.Settings.ProfileDirectory;
        public bool IncludeDevelopment => Launcher.Settings.IncludeDevelopment;

        // On Property Change

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
