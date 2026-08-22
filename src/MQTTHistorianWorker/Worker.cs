using Microsoft.Extensions.Options;
using MQTTHistorianWorker.Models;
using MQTTHistorianWorker.Services.MQTTSubscriberService;

namespace MQTTHistorianWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly MqttServerConfigModel? _mqttServerConfig;
        private readonly IMqttSubscriberService _mqttSubscriberService;

        public Worker(ILogger<Worker> logger,IMqttSubscriberService mqttSubscriber ,IOptions<ApplicationConfigModel> config)
        {
            _logger = logger;
            _mqttServerConfig = config.Value.MqttServer;
            _mqttSubscriberService = mqttSubscriber;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            await _mqttSubscriberService.ConfigureMqttClientAsync();

            await _mqttSubscriberService.SubscribeMessagesAsync();

            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    if (_logger.IsEnabled(LogLevel.Information))
            //    {
            //        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            //    }

               

            //   await Task.Delay(10000, stoppingToken);
            //}
        }
    }
}
