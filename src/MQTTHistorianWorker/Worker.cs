using Microsoft.Extensions.Options;
using MQTTHistorianWorker.Models;

namespace MQTTHistorianWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly MqttServerConfigModel? _mqttServerConfig;

        public Worker(ILogger<Worker> logger, IOptions<ApplicationConfigModel> config)
        {
            _logger = logger;
            _mqttServerConfig = config.Value.MqttServer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
