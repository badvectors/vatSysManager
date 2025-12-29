using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Security.Principal;
using System.Windows;
using vatSysLauncher.Models;

namespace vatSysLauncher.Controllers
{
    public class Utility
    {
        private static readonly HttpClient HttpClient = new();

        public static UpdaterResult EmptyDirectory(string directory)
        {
            var result = new UpdaterResult();

            if (Directory.Exists(directory))
            {
                result.Add($"Emptying directory: {directory}.");

                try
                {
                    DirectoryInfo dir = new(directory);

                    foreach (var file in dir.GetFiles())
                    {
                        file.Delete();
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    RestartAsAdministrator();

                    result.Add($"Could not empty directory as administrator access was not provided");

                    return result;
                }
                catch (Exception ex)
                {
                    result.Add($"Could not directory directory: {ex.Message}");

                    return result;
                }
            }

            result.Success = true;

            return result;
        }

        public static UpdaterResult CreateDirectory(string directory)
        {
            var result = new UpdaterResult();

            if (Directory.Exists(directory))
            {
                return EmptyDirectory(directory);
            }

            try
            {
                Directory.CreateDirectory(directory);
            }
            catch (UnauthorizedAccessException)
            {
                RestartAsAdministrator();

                result.Add($"Could not create directory as administrator access was not provided");

                return result;
            }
            catch (Exception ex)
            {
                result.Add($"Could not create directory: {ex.Message}");

                return result;
            }

            result.Success = true;

            return result;
        }

        public static UpdaterResult DeleteDirectory(string directory)
        {
            var result = new UpdaterResult();

            if (Directory.Exists(directory))
            {
                result.Add($"Deleting directory: {directory}");

                try
                {
                    DirectoryInfo dir = new(directory);

                    SetAttributesNormal(dir);

                    dir.Delete(true);
                }
                catch (UnauthorizedAccessException)
                {
                    RestartAsAdministrator();

                    result.Add($"Could not delete directory as administrator access was not provided");

                    return result;
                }
                catch (Exception ex)
                {
                    result.Add($"Could not delete directory: {ex.Message}");

                    return result;
                }
            }

            result.Success = true;

            return result;
        }

        public static UpdaterResult ExtractZip(string zipFile, string toDirectory)
        {
            var result = new UpdaterResult();

            result.Add("Extracting plugin.");

            try
            {
                ZipFile.ExtractToDirectory(zipFile, toDirectory);
            }
            catch (Exception ex)
            {
                result.Add($"Could not extract file: {ex.Message}");

                if (ex.InnerException != null)
                {
                    result.Add($"-> {ex.InnerException.Message}");
                }

                return result;
            }

            var subdirectories = Directory.GetDirectories(toDirectory);

            if (subdirectories.Length == 1 && Directory.GetFiles(toDirectory).Length == 0)
            {
                var files = Directory.GetFiles(subdirectories[0]);

                foreach (var file in files)
                {
                    var fileName = file.Split("\\").Last();
                    File.Move(file, Path.Combine(toDirectory, fileName));
                }
            }

            foreach (var file in Directory.GetFiles(toDirectory))
            {
                File.SetAttributes(file, FileAttributes.Normal);
            }

            result.Add("Extract completed.");

            result.Success = true;

            return result;
        }

        public static UpdaterResult DeleteFile(string file)
        {
            var result = new UpdaterResult();

            if (!File.Exists(file))
            {
                result.Add("File does not exist.");

                return result;
            }

            try
            {
                File.Delete(file);
            }
            catch (Exception ex)
            {
                result.Add($"Could not delete file: {ex.Message}");

                return result;
            }

            result.Add("File deleted.");

            result.Success = true;

            return result;
        }

        public static async Task<UpdaterResult> DownloadFile(string url, string name = "Temp.zip")
        {
            var result = new UpdaterResult();

            if (string.IsNullOrWhiteSpace(url))
            {
                result.Add("No download link was found.");

                return result;
            }

            result.Add($"Downloading from: {url}.");

            using (var downloadResponse = await HttpClient.GetAsync(url))
            {
                if (!downloadResponse.IsSuccessStatusCode)
                {
                    result.Add($"Could not download file: {downloadResponse.StatusCode}.");

                    return result;
                }

                using (var stream = await downloadResponse.Content.ReadAsStreamAsync())
                using (var file = File.OpenWrite(Path.Combine(Launcher.WorkingDirectory, name)))
                {
                    stream.CopyTo(file);
                }
            }

            result.Add("Download completed.");

            result.Success = true;

            return result;
        }

        public static bool IsRunningAsAdministrator()
        {
            // Get current Windows user
            WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();

            // Get current Windows user principal
            WindowsPrincipal windowsPrincipal = new(windowsIdentity);

            // Return TRUE if user is in role "Administrator"
            return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static void RestartAsAdministrator()
        {
            if (IsRunningAsAdministrator()) return;

            if (File.Exists(Launcher.RestartFile))
            {
                File.Delete(Launcher.RestartFile);
            }

            File.WriteAllLines(Launcher.RestartFile, Updater.GetCurrentCommands());

            // Setting up start info of the new process of the same application
            ProcessStartInfo processStartInfo = new(Environment.ProcessPath)
            {
                UseShellExecute = true,
                Verb = "runas"
            };

            try
            {
                // Start the application as new process
                Process.Start(processStartInfo);

                // Shut down the current (old) process
                Application.Current.Shutdown();
            }
            catch
            {
                string messageBoxText = "You must grant administrator access to install plugins in the base directory.";
                string caption = "vatSys Launcher";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Error;
                MessageBoxResult result;
                result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
                switch (result)
                {
                    case MessageBoxResult.OK:
                        break;
                }
                return;
            }
        }

        public static void SetAttributesNormal(DirectoryInfo dir)
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

        public static string GetFileVersion()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileVersion;
        }

        public static async Task CheckVersion()
        {
            var versionResponse = await Launcher.HttpClient.GetAsync(Launcher.VersionUrl);

            if (!versionResponse.IsSuccessStatusCode) return;

            var content = await versionResponse.Content.ReadAsStringAsync();

            try
            {
                var version = JsonConvert.DeserializeObject<LauncherVersion>(content);

                if (version.Version == GetFileVersion()) return;

                string messageBoxText = $"You must update vatSys Launcher to version {version.Version} to continue.";
                string caption = "vatSys Launcher";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Exclamation;
                MessageBoxResult result;
                result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
                switch (result)
                {
                    case MessageBoxResult.OK:
                        await Updater.UpdateSelf(version.DownloadUrl);
                        break;
                }
                return;
            }
            catch { }
        }
    }
}
