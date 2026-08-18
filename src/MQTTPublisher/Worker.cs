using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Protocol;
using MQTTPublisher.Helper;
using MQTTPublisher.Models;
using System.Runtime.CompilerServices;

namespace MQTTPublisher
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private IMqttClient _mqttClient;
        private readonly GetRandomData _getRandomData;
        private readonly IOptions<EdgePoints> _edgePointsOptions;

        public const string CLIENT_ID = "MQTTPublisherClient";

        public Worker(ILogger<Worker> logger, GetRandomData getRandomData, IMqttClient mqttClient,IOptions<EdgePoints> edgePointsOptions)
        {
            _logger = logger;
            _getRandomData = getRandomData;
            _mqttClient = mqttClient;
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

        private async Task PublishMessage(List<EdgePointsDetails> edgePointsDetails)
        {
            foreach (var sensor in edgePointsDetails)
            {
                var data = _getRandomData.GetRandomInteger(0,100);
                var topic = sensor.Topic;
                var payload = $"{sensor.Name}: {data}";

                var msgPayload = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithQualityOfServiceLevel(_edgePointsOptions.Value.QualityOfService.GetMqttQualityOfService())
                    .WithPayload(payload)
                    .WithRetainFlag(_edgePointsOptions.Value.WillRetain)
                    .Build();

                //send the message to the MQTT broker
                var result = await _mqttClient.PublishAsync(msgPayload, CancellationToken.None);

                if (result.ReasonCode == MqttClientPublishReasonCode.Success)
                {
                    _logger.LogInformation("Published message to topic: {topic} with payload: {payload}", topic, payload);
                }
                else
                {
                    _logger.LogError("Failed to publish message to topic: {topic}. Reason: {reasonCode}", topic, result.ReasonCode);
                }
            }
        }

        public MqttQualityOfServiceLevel GetMqttQualityOfService(int no)
        {
            //switch (no)
            //{
            //    case 0:
            //        return MqttQualityOfServiceLevel.AtMostOnce;
            //    case 1:
            //        return MqttQualityOfServiceLevel.AtLeastOnce;
            //    case 2:
            //        return MqttQualityOfServiceLevel.ExactlyOnce;
            //    default:
            //        return MqttQualityOfServiceLevel.AtMostOnce;
            //}
            return no.GetMqttQualityOfService();
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_mqttClient.IsConnected)
            {
                await PublishDisconnectMessage();
                await _mqttClient.DisconnectAsync();
                _logger.LogInformation("Disconnected from MQTT broker.");
            }
            await base.StopAsync(cancellationToken);
        }

        public async Task PublishDisconnectMessage()
        {
            if (_mqttClient.IsConnected)
            {
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(_edgePointsOptions.Value.StatusTopic)
                    .WithPayload(_edgePointsOptions.Value.StatusOfflineMessage)
                    .WithRetainFlag(_edgePointsOptions.Value.WillRetain)
                    .Build();
                await _mqttClient.PublishAsync(message,CancellationToken.None);
                _logger.LogInformation("Published disconnect message to topic: {topic}", _edgePointsOptions.Value.StatusTopic);
            }
        }

        public async Task PublishConnectMessage()
        {
            if (_mqttClient.IsConnected)
            {
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(_edgePointsOptions.Value.StatusTopic)
                    .WithPayload(_edgePointsOptions.Value.StatusOnlineMessage)
                    .WithRetainFlag(_edgePointsOptions.Value.WillRetain)
                    .Build();
                await _mqttClient.PublishAsync(message, CancellationToken.None);
                _logger.LogInformation("Published connect message to topic: {topic}", _edgePointsOptions.Value.StatusTopic);
            }
        }

        private async Task ConfigureMqttClient()
        {
            var msgOptions = new MqttClientOptionsBuilder().
                WithTcpServer(_edgePointsOptions.Value.BrokerAddress, _edgePointsOptions.Value.Port)
                .WithClientId(CLIENT_ID)
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(30))
                .WithWillTopic(_edgePointsOptions.Value.StatusTopic)
                .WithWillPayload(_edgePointsOptions.Value.StatusOfflineMessage)
                .WithWillQualityOfServiceLevel(_edgePointsOptions.Value.QualityOfService.GetMqttQualityOfService())
                .WithWillRetain(_edgePointsOptions.Value.WillRetain)
                .Build();

            // Connect to the MQTT broker
            var connResult = await _mqttClient.ConnectAsync(msgOptions);

            if (connResult.ResultCode == MqttClientConnectResultCode.Success)
            {
                _logger.LogInformation("Connected to MQTT broker at {brokerAddress}:{port}", _edgePointsOptions.Value.BrokerAddress, _edgePointsOptions.Value.Port);
                await PublishConnectMessage();
            }
            else
            {
                _logger.LogError("Failed to connect to MQTT broker: {resultCode}", connResult.ResultCode);
            }
        }
    }
}
