using Microsoft.AspNetCore.Mvc;

namespace vatSysLauncher.Web.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PluginsController(IPluginService pluginService) : ControllerBase
    {
        private readonly IPluginService _pluginService = pluginService;

        [HttpGet]
        public async Task<List<PluginResponse>> Get()
        {
            try
            {
                return await _pluginService.Get();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return [];
            }
        }

        [HttpGet, Route("LastUpdate")]
        public DateTime LastUpdate()
        {
            try
            {
                return _pluginService.LastRefresh();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return DateTime.MinValue;
            }
        }

        [HttpGet, Route("ForceUpdate")]
        public async Task<DateTime> ForceUpdate()
        {
            try
            {
                await _pluginService.Update();

                return _pluginService.LastRefresh();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return DateTime.MinValue;
            }
        }
    }
}