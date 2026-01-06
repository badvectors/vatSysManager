using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using vatSysLauncher.Controllers;

namespace vatSysManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = Launcher.MainViewModel;
            _ = Init();
        }

        private async Task Init()
        {
            Settings.Init();

            Launcher.SetCanvas("Home");

            Launcher.SetLoading(true);

            await Utility.CheckVersion();

            await Profiles.Init();

            await Plugins.Init();

            await Launcher.CheckForRestart();

            Launcher.SetLoading(false);

            VatSys.Init();

            Launcher.GetChanges();

            Utility.DeleteDirectory(Launcher.WorkingDirectory);
        }

        private void VatSysCloseButton_Click(object sender, RoutedEventArgs e)
        {
            VatSys.Close();
        }

        private async void VatSysLaunchButton_Click(object sender, RoutedEventArgs e)
        {
            await VatSys.Launch();
        }

        private void SetupButton_Click(object sender, RoutedEventArgs e)
        {
            Launcher.SetCanvas("Setup");
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            Launcher.SetCanvas("Home");
        }

        private void ProfilesButton_Click(object sender, RoutedEventArgs e)
        {
            Launcher.SetCanvas("Profiles");
        }

        private void PluginsButton_Click(object sender, RoutedEventArgs e)
        {
            Launcher.SetCanvas("Plugins");
        }

        private async void UpdaterButton_Click(object sender, RoutedEventArgs e)
        {
            var command = ((Button)sender).Tag.ToString();

            await Updater.Run(command);
        }

        private async void PluginInstallButton_Click(object sender, RoutedEventArgs e)
        {
            if (PluginsLocationsComboBox.SelectedValue == null || PluginsOptionsComboBox.SelectedValue == null) return;

            var location = PluginsLocationsComboBox.SelectedValue.ToString();

            var pluginName = PluginsOptionsComboBox.SelectedValue.ToString();

            if (location == null || pluginName == null) return;

            if (location == Launcher.PluginsBaseDirectoryName)
            {
                location = Launcher.PluginsBaseDirectory;
            }
            else
            {
                location = $"{Launcher.Settings.ProfileDirectory}\\{location}\\Plugins";
            }

            var pluginResponse = Launcher.PluginsAvailable.FirstOrDefault(x => x.Name == pluginName);

            if (pluginResponse == null) return;

            var installCommand = $"Install|Plugin|{pluginResponse.Name}|{location}\\{pluginResponse.DirectoryName}";

            await Updater.Run(installCommand);
        }

        private void BaseDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new OpenFolderDialog();

            if (folderDialog.ShowDialog() == true)
            {
                Launcher.Settings.BaseDirectory = folderDialog.FolderName;

                Settings.Save();

                BaseDirectoryTextBox.Text = Launcher.Settings.BaseDirectory;
            }
        }

        private void ProfileDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new OpenFolderDialog();

            if (folderDialog.ShowDialog() == true)
            {
                Launcher.Settings.ProfileDirectory = folderDialog.FolderName;

                Settings.Save();

                _ = Profiles.Init();

                ProfileDirectoryTextBox.Text = Launcher.Settings.ProfileDirectory;
            }
        }

        private void UpdaterLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox) return;
            var textBox = (TextBox)e.Source;
            textBox.CaretIndex = textBox.Text.Length;
            textBox.ScrollToEnd();
        }
    }
}