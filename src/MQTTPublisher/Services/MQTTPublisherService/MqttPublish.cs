using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTPublisher.Helper;
using MQTTPublisher.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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

        private ConcurrentQueue<EdgePointsDetails> EdgePointsQueue = new ConcurrentQueue<EdgePointsDetails>();

        public MqttPublish(ILogger<MqttPublish> logger,GetRandomData getRandomData,IOptions<EdgePoints> options,IMqttClient mqttClient)
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
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(600))
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

            var result = await _mqttClient.PublishAsync(msgOptions, cancellationToken);

            //Disconnect the client if it's a disconnect message
            if(!isConnect)
                await _mqttClient.DisconnectAsync();

            return result;
        }

        public async Task<MqttClientPublishResult> PublishMessageAsync(EdgePointsDetails edgePointsDetails, CancellationToken cancellationToken = default)
        {
            //ArgumentNullException.ThrowIfNull(_mqttClient);

            //if (!_mqttClient.IsConnected)
            //    throw new InvalidOperationException("Cannot publish message because the MQTT client is not connected.");

            EdgePointsQueue.Enqueue(edgePointsDetails);

            //var data = _randomData.GetRandomInteger(0,100);
            //var payload = new
            //{
            //    Name = edgePointsDetails.Name,
            //    Value = data,
            //    TimeStamp = DateTime.UtcNow.ToString("o") // ISO 8601 format
            //};

            //var jsonPayload = JsonSerializer.Serialize(payload);

            //var msgOptions = new MqttApplicationMessageBuilder()
            //    .WithTopic(edgePointsDetails.Topic)
            //    .WithPayload(jsonPayload)
            //    .WithQualityOfServiceLevel(_edgePoints.QualityOfService.GetMqttQualityOfService())
            //    .WithRetainFlag(_edgePoints.WillRetain)
            //    .Build();

            //return await _mqttClient.PublishAsync(msgOptions,cancellationToken);

            return await PublishQueuedMessagesAsync(cancellationToken);
        }

        private async Task<MqttClientPublishResult> PublishQueuedMessagesAsync(CancellationToken cancellationToken = default)
        {
            //ArgumentNullException.ThrowIfNull(_mqttClient);

            //if (!_mqttClient.IsConnected)
            //    throw new InvalidOperationException("Cannot publish message because the MQTT client is not connected.");
            MqttClientPublishResult? lastpublishResult = null;

            if(_mqttClient is null || !_mqttClient.IsConnected)
            {
                _logger.LogError("Cannot publish message because the MQTT client is not connected.");
                return lastpublishResult!;
            }

            while (EdgePointsQueue.TryDequeue(out var edgePointsDetails))
            {
                try
                {
                    var data = _randomData.GetRandomInteger(0, 100);
                    var payload = new
                    {
                        Name = edgePointsDetails.Name,
                        Value = data,
                        TimeStamp = DateTime.UtcNow.ToString("o") // ISO 8601 format
                    };

                    var jsonPayload = JsonSerializer.Serialize(payload);

                    var msgOptions = new MqttApplicationMessageBuilder()
                        .WithTopic(edgePointsDetails.Topic)
                        .WithPayload(jsonPayload)
                        .WithQualityOfServiceLevel(_edgePoints.QualityOfService.GetMqttQualityOfService())
                        .WithRetainFlag(_edgePoints.WillRetain)
                        .Build();

                    lastpublishResult = await _mqttClient.PublishAsync(msgOptions, cancellationToken);
                }
                catch (Exception)
                {
                    _logger.LogError("Failed to publish message for {Name} to topic: {Topic}. Re-enqueuing the message.", edgePointsDetails.Name, edgePointsDetails.Topic);
                    //lastpublishResult = await PublishQueuedMessagesAsync(cancellationToken);

                    EdgePointsQueue.Enqueue(edgePointsDetails);

                    break;
                }
            }

            return lastpublishResult!;
        }
    }
}
