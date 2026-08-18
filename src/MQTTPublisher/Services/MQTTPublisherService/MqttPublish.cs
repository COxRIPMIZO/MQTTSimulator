using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTPublisher.Helper;
using MQTTPublisher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTPublisher.Services.MQTTPublisherService
{
    public class MqttPublish : IMqttPublish
    {
        private readonly ILogger _logger;
        private readonly GetRandomData _randomData;
        private readonly EdgePoints _edgePoints;
        private IMqttClient _mqttClient;

        public const string CLIENT_ID = "MQTTPublisherClient";

        public MqttPublish(ILogger logger,GetRandomData getRandomData,IOptions<EdgePoints> options,IMqttClient mqttClient)
        {
            _logger = logger;
            _randomData = getRandomData;
            _edgePoints = options.Value;
            _mqttClient = mqttClient;
        }

        public async Task<MqttClientConnectResult> ConfigureMqttClientAsync(CancellationToken cancellationToken = default)
        {
            var clientConfigOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(_edgePoints.BrokerAddress, _edgePoints.Port)
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(30))
                .WithClientId(CLIENT_ID)
                .WithWillTopic(_edgePoints.StatusTopic)
                .WithWillPayload(_edgePoints.StatusOfflineMessage)
                .WithWillQualityOfServiceLevel(_edgePoints.QualityOfService.GetMqttQualityOfService())
                .WithWillRetain(_edgePoints.WillRetain)
                .Build();

            return await _mqttClient.ConnectAsync(clientConfigOptions, cancellationToken);
        }

        public async Task<MqttClientPublishResult> PublishStatusMessageAsync(bool isConnect,CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(_mqttClient);

            if (!_mqttClient.IsConnected)
                throw new InvalidOperationException("Cannot publish message because the MQTT client is not connected.");

            var payload = isConnect ? _edgePoints.StatusOnlineMessage : _edgePoints.StatusOfflineMessage;

            var msgOptions = new MqttApplicationMessageBuilder()
                .WithTopic(_edgePoints.StatusTopic)
                .WithPayload(payload)
                .WithRetainFlag(_edgePoints.WillRetain)
                .Build();

            return await _mqttClient.PublishAsync(msgOptions, cancellationToken);
        }

        public async Task<MqttClientPublishResult> PublishMessageAsync(EdgePointsDetails edgePointsDetails, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(_mqttClient);

            if (!_mqttClient.IsConnected)
                throw new InvalidOperationException("Cannot publish message because the MQTT client is not connected.");

            var data = _randomData.GetRandomInteger(0,100);
            var payload = $"{{\"Name\" : \"{edgePointsDetails.Name}\",\"Value\" : \"{data}\" }}";

            var msgOptions = new MqttApplicationMessageBuilder()
                .WithTopic(edgePointsDetails.Topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(_edgePoints.QualityOfService.GetMqttQualityOfService())
                .WithRetainFlag(_edgePoints.WillRetain)
                .Build();

            return await _mqttClient.PublishAsync(msgOptions,cancellationToken);
        }
    }
}
