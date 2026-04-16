using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace vatSysLauncher.Web
{
    public interface IPluginService
    {
        Task<List<PluginResponse>> Get();
        DateTime LastRefresh();
        Task Update();
    }

    public class PluginsService : IPluginService
    {
        private static readonly string _pluginsUrl = "https://raw.githubusercontent.com/badvectors/vatSysLauncher/refs/heads/master/vatSysLauncher/Plugins.json";
        private static readonly HttpClient _httpClient = new();
        private static readonly TimeSpan _refreshTime = TimeSpan.FromMinutes(15);

        private static readonly string _pluginsJson = "plugins.json";
        private static readonly string _lastRefreshTxt = "last_refresh.txt";

        public PluginsService() 
        {
            Init();

            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("vatSysLauncher", "1.19.0"));

            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
        }

        private static void Init()
        {
            if (!File.Exists(_pluginsJson))
            {
                var plugins = new List<PluginResponse>();

                var pluginsJson = JsonConvert.SerializeObject(plugins);

                File.WriteAllText(_pluginsJson, pluginsJson);

            }

            if (!File.Exists(_lastRefreshTxt))
            {
                File.WriteAllText(_lastRefreshTxt, DateTime.MinValue.ToString());
            }
        }

        public async Task<List<PluginResponse>> Get()
        {
            if (ShouldUpdate())
            {
                await Update();
            }

            return GetPlugins();
        }

        private List<PluginResponse> GetPlugins()
        {
            var pluginsJson = File.ReadAllText(_pluginsJson);

            var plugins = JsonConvert.DeserializeObject<List<PluginResponse>>(pluginsJson);

            return plugins;
        }

        public DateTime LastRefresh()
        {
            var dateTimeOK = DateTime.TryParse(File.ReadAllText(_lastRefreshTxt), out DateTime lastUpdate);

            if (!dateTimeOK)
            {
                File.WriteAllText(_lastRefreshTxt, DateTime.MinValue.ToString());

                return DateTime.MinValue;
            }

            return lastUpdate;
        }

        private bool ShouldUpdate()
        {
            if (GetPlugins().Count == 0) return true;

            if (LastRefresh().Add(_refreshTime) < DateTime.UtcNow) return true;

            return false;
        }

        public async Task Update()
        {
            File.WriteAllText(_lastRefreshTxt, DateTime.UtcNow.ToString());

            var available = await GetAvailable();

            var current = GetPlugins();

            // Add any new plugins.
            foreach (var plugin in available)
            {
                if (current.Any(x => x.Name == plugin.Name)) continue;

                current.Add(plugin);
            }

            // Remove any deleted plugins.
            foreach (var plugin in current.ToList())
            {
                if (available.Any(x => x.Name == plugin.Name)) continue;

                current.Remove(plugin);
            }

            // Get versions.
            foreach (var plugin in current)
            {
                await GetVersion(plugin);
            }

            var pluginsJson = JsonConvert.SerializeObject(current);

            File.WriteAllText(_pluginsJson, pluginsJson);
        }

        private static async Task<List<PluginResponse>> GetAvailable()
        {
            var plugins = new List<PluginResponse>();

            try
            {
                var response = await _httpClient.GetAsync(_pluginsUrl);

                if (!response.IsSuccessStatusCode) return plugins;

                var content = await response.Content.ReadAsStringAsync();

                var pluginResponses = JsonConvert.DeserializeObject<List<PluginResponse>>(content);

                plugins.AddRange(pluginResponses);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return plugins;
        }

        private static async Task<PluginResponse> GetVersion(PluginResponse pluginResponse)
        {
            if (pluginResponse == null) return null;

            if (pluginResponse.Remove == true) return null;

            try
            {
                var latestPage = await _httpClient.GetAsync(pluginResponse.LatestUrl);

                if (!latestPage.IsSuccessStatusCode)
                {
                    Console.WriteLine($"{latestPage.StatusCode} {latestPage.Content}");

                    return pluginResponse;
                }

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
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
            }

            return pluginResponse;
        }
    }
}
