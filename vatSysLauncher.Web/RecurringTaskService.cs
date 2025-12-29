namespace vatSysLauncher.Web
{
    public class RecurringTaskService(IPluginService pluginService) : BackgroundService
    {
        private readonly IPluginService _pluginService = pluginService;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine($"Update task run at: {DateTimeOffset.Now}");

                await _pluginService.Update();

                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
        }
    }
}
