using Microsoft.AspNetCore.Mvc;

namespace vatSysLauncher.Web.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PluginsController(IPluginService pluginService) : ControllerBase
    {
        private readonly IPluginService _pluginService = pluginService;

        [HttpGet]
        public List<PluginResponse> Get()
        {
            return _pluginService.Get();
        }

        [HttpGet, Route("LastUpdate")]
        public DateTime LastUpdate()
        {
            return _pluginService.LastRefresh();
        }

        [HttpGet, Route("ForceUpdate")]
        public async Task<DateTime> ForceUpdate()
        {
            await _pluginService.Update();

            return _pluginService.LastRefresh();
        }
    }
}