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
        private static string _pluginsUrl => "https://raw.githubusercontent.com/badvectors/vatSysLauncher/refs/heads/master/vatSysLauncher/Plugins.json";

        private static HttpClient _httpClient = new();
        private static DateTime _lastRefresh = DateTime.MinValue;
        private static TimeSpan _refreshTime = TimeSpan.FromMinutes(15);
        private static List<PluginResponse> _plugins = [];
        
        public PluginsService() 
        {
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("vatSysLauncher", "1.19.0"));

            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
        }

        public async Task<List<PluginResponse>> Get()
        {
            if (ShouldUpdate())
            {
                await Update();
            }
            return _plugins;
        }
        public DateTime LastRefresh() => _lastRefresh;

        private bool ShouldUpdate()
        {
            if (_plugins.Count == 0) return true;

            if (_lastRefresh.Add(_refreshTime) < DateTime.UtcNow) return true;

            return false;
        }

        public async Task Update()
        {
            _lastRefresh = DateTime.UtcNow;

            var available = await GetAvailable();

            // Add any new plugins.
            foreach (var plugin in available)
            {
                if (_plugins.Any(x => x.Name == plugin.Name)) continue;

                _plugins.Add(plugin);
            }

            // Remove any deleted plugins.
            foreach (var plugin in _plugins.ToList())
            {
                if (available.Any(x => x.Name == plugin.Name)) continue;

                _plugins.Remove(plugin);
            }

            // Get versions.
            foreach (var plugin in _plugins)
            {
                await GetVersion(plugin);
            }
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
