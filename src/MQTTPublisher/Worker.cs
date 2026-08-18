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
        //private IMqttClient _mqttClient;
        //private readonly GetRandomData _getRandomData;
        private readonly IOptions<EdgePoints> _edgePointsOptions;

        //public const string CLIENT_ID = "MQTTPublisherClient";
        private readonly IMqttPublish _mqttPublisher;
        
        public Worker(ILogger<Worker> logger, IMqttPublish mqttPublisher,IOptions<EdgePoints> edgePointsOptions)
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

                await Task.Delay(500, stoppingToken);
            }
        }

        private async Task PublishMessage(List<EdgePointsDetails> edgePointsDetails)
        {
            //Parallel.ForEach(edgePointsDetails, async sensor => 
            //{
            //    //send the message to the MQTT broker
            //    //var result = await _mqttClient.PublishAsync(msgPayload, CancellationToken.None);
            //    var result = await _mqttPublisher.PublishMessageAsync(sensor, CancellationToken.None);

            //    if (result.ReasonCode == MqttClientPublishReasonCode.Success)
            //    {
            //        _logger.LogInformation("Published message to topic: {topic} with payload: {payload}", sensor.Topic, sensor.Name);
            //    }
            //    else
            //    {
            //        _logger.LogError("Failed to publish message to topic: {topic}. Reason: {reasonCode}", sensor.Topic, result.ReasonCode);
            //    }

            //    await Task.Delay(50, CancellationToken.None);
            //});

            foreach (var sensor in edgePointsDetails)
            {
                //var data = _getRandomData.GetRandomInteger(0,100);
                //var topic = sensor.Topic;
                //var payload = $"{sensor.Name}: {data}";

                //var msgPayload = new MqttApplicationMessageBuilder()
                //    .WithTopic(topic)
                //    .WithQualityOfServiceLevel(_edgePointsOptions.Value.QualityOfService.GetMqttQualityOfService())
                //    .WithPayload(payload)
                //    .WithRetainFlag(_edgePointsOptions.Value.WillRetain)
                //    .Build();


                //send the message to the MQTT broker
                //var result = await _mqttClient.PublishAsync(msgPayload, CancellationToken.None);
                var result = await _mqttPublisher.PublishMessageAsync(sensor, CancellationToken.None);

                if (result.ReasonCode == MqttClientPublishReasonCode.Success)
                {
                    _logger.LogInformation("Published message to topic: {topic} with payload: {payload}", sensor.Topic, sensor.Name);
                }
                else
                {
                    _logger.LogError("Failed to publish message to topic: {topic}. Reason: {reasonCode}", sensor.Topic, result.ReasonCode);
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            //if (_mqttClient.IsConnected)
            //{
            //    await PublishDisconnectMessage();
            //    await _mqttClient.DisconnectAsync();

            var disconnectResult = await _mqttPublisher.PublishStatusMessageAsync(false, cancellationToken);

            if(disconnectResult.IsSuccess)
                _logger.LogInformation("Disconnected from MQTT broker.");
            else
                _logger.LogError("Failed to disconnect from MQTT broker. Reason: {reasonCode}", disconnectResult.ReasonCode);
            //}
            await base.StopAsync(cancellationToken);
        }

        //public async Task PublishDisconnectMessage()
        //{
        //    if (_mqttClient.IsConnected)
        //    {
        //        var message = new MqttApplicationMessageBuilder()
        //            .WithTopic(_edgePointsOptions.Value.StatusTopic)
        //            .WithPayload(_edgePointsOptions.Value.StatusOfflineMessage)
        //            .WithRetainFlag(_edgePointsOptions.Value.WillRetain)
        //            .Build();
        //        await _mqttClient.PublishAsync(message,CancellationToken.None);
        //        _logger.LogInformation("Published disconnect message to topic: {topic}", _edgePointsOptions.Value.StatusTopic);
        //    }
        //}

        public async Task PublishConnectMessage()
        {
            //if (_mqttClient.IsConnected)
            //{
            //    var message = new MqttApplicationMessageBuilder()
            //        .WithTopic(_edgePointsOptions.Value.StatusTopic)
            //        .WithPayload(_edgePointsOptions.Value.StatusOnlineMessage)
            //        .WithRetainFlag(_edgePointsOptions.Value.WillRetain)
            //        .Build();
            //    await _mqttClient.PublishAsync(message, CancellationToken.None);

            var connectResult = await _mqttPublisher.PublishStatusMessageAsync(true, CancellationToken.None);

            if(connectResult.IsSuccess)
                _logger.LogInformation("Connected from MQTT broker.");
            else
                _logger.LogError("Failed to connect MQTT broker.");
            //}
        }

        private async Task ConfigureMqttClient()
        {
            //var msgOptions = new MqttClientOptionsBuilder().
            //    WithTcpServer(_edgePointsOptions.Value.BrokerAddress, _edgePointsOptions.Value.Port)
            //    .WithClientId(CLIENT_ID)
            //    .WithKeepAlivePeriod(TimeSpan.FromSeconds(30))
            //    .WithWillTopic(_edgePointsOptions.Value.StatusTopic)
            //    .WithWillPayload(_edgePointsOptions.Value.StatusOfflineMessage)
            //    .WithWillQualityOfServiceLevel(_edgePointsOptions.Value.QualityOfService.GetMqttQualityOfService())
            //    .WithWillRetain(_edgePointsOptions.Value.WillRetain)
            //    .Build();

            //// Connect to the MQTT broker
            //var connResult = await _mqttClient.ConnectAsync(msgOptions);

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
