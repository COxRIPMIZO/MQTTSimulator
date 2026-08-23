using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Protocol;
using MQTTPublisher.Helper;
using MQTTPublisher.Models;
using MQTTPublisher.Services.MQTTPublisherService;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace MQTTPublisher
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IOptions<EdgePointsModel> _edgePointsOptions;
        private readonly IMqttPublish _mqttPublisher;
        
        public Worker(ILogger<Worker> logger, IMqttPublish mqttPublisher,IOptions<EdgePointsModel> edgePointsOptions)
        {
            _logger = logger;
            _mqttPublisher = mqttPublisher;
            _edgePointsOptions = edgePointsOptions;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ConfigureMqttClient();

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }

                //mqtt data sending logic
                await PublishMessage(_edgePointsOptions.Value.Sensors);

                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task PublishMessage(List<EdgePointsDetailModel> edgePointsDetails)
        {
            foreach (var sensor in edgePointsDetails)
            {
                var result = await _mqttPublisher.PublishMessageAsync(sensor, CancellationToken.None);

                if (result.ReasonCode == MqttClientPublishReasonCode.Success)
                {
                    //_logger.LogInformation("Published message to topic: {topic} with payload: {payload} {Value}", sensor.Topic, sensor.Name,result.PacketIdentifier.Value);
                }
                else
                {
                    _logger.LogError("Failed to publish message to topic: {topic}. Reason: {reasonCode}", sensor.Topic, result.ReasonCode);
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            var disconnectResult = await _mqttPublisher.PublishStatusMessageAsync(false, cancellationToken);

            if(disconnectResult.IsSuccess)
                _logger.LogInformation("Disconnected from MQTT broker.");
            else
                _logger.LogError("Failed to disconnect from MQTT broker. Reason: {reasonCode}", disconnectResult.ReasonCode);
            
            await base.StopAsync(cancellationToken);
        }

        public async Task PublishConnectMessage()
        {
            var connectResult = await _mqttPublisher.PublishStatusMessageAsync(true, CancellationToken.None);

            if(connectResult.IsSuccess)
                _logger.LogInformation("Connected from MQTT broker.");
            else
                _logger.LogError("Failed to connect MQTT broker.");
        }

        private async Task ConfigureMqttClient()
        {
            var connectionResult = await _mqttPublisher.ConfigureMqttClientAsync(CancellationToken.None);

            if (connectionResult.ResultCode == MqttClientConnectResultCode.Success)
            {
                _logger.LogInformation("Connected from MQTT broker.");
                await PublishConnectMessage();
            }
            else
            {
                _logger.LogError("Failed to connect MQTT broker.");
            }
        }
    }
}
