using Microsoft.Extensions.Options;
using MQTTHistorianWorker.Models;
using MQTTHistorianWorker.Services.MQTTHistorianService.Interfaces;
using MQTTHistorianWorker.Services.MQTTHistorianService.Models;
using MQTTnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MQTTHistorianWorker.Services.MQTTSubscriberService
{
    public class MqttSubscriberService : IMqttSubscriberService
    {
        private readonly ILogger<MqttSubscriberService> _logger;
        private IMqttClient _mqttClient;
        private readonly IOptions<ApplicationConfigModel> _options;
        private readonly MqttServerConfigModel? _mqttServerConfigModel;
        private readonly List<SensorsTopics> _sensorTopics;

        private readonly RegexOptions _regexOptions = RegexOptions.Compiled;
        private readonly string _regexPattern = "^sensor/(?<id>[^/]+)/data$";
        private readonly Regex _regex;

        private readonly IMqttHistorianService _mqttHistorianService;

        public MqttSubscriberService(ILogger<MqttSubscriberService> logger, IMqttClient mqttClient, IOptions<ApplicationConfigModel> options,IMqttHistorianService mqttHistorian)
        {
            _mqttHistorianService = mqttHistorian;
            _logger = logger;
            _mqttClient = mqttClient;
            _options = options;
            _mqttServerConfigModel = options.Value.MqttServer;

            _sensorTopics = options.Value.SensorsTopics;

            //regex pattern
            _regex = new Regex(_regexPattern, _regexOptions);
        }

        public async Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(_mqttClient);

            await _mqttClient.DisconnectAsync(cancellationToken : cancellationToken);
            
            _logger.LogInformation("Client disconnected successfully.");
        }

        public async Task<MqttClientConnectResult> ConfigureMqttClientAsync(CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(_mqttClient);

            var mqttOptions = new MqttClientOptionsBuilder().
                WithTcpServer(_mqttServerConfigModel?.BaseAddress, _mqttServerConfigModel?.Port)
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(600))
                .WithClientId(_options.Value.UniqueClientID)
                .Build();

            try
            {
                var response = await _mqttClient.ConnectAsync(mqttOptions, cancellationToken);

                _logger.LogInformation("Client connected successfully.");

                return response;
            }
            catch (Exception)
            {
                _logger.LogWarning("Error occured while connecting client.");
                return new MqttClientConnectResult();
            }
        }

        public async Task<MqttClientSubscribeResult> SubscribeMessagesAsync(CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(_mqttClient);

            //subscribe event
            SubscribeApplicationMessageReceived();

            //options builder
            var msgOptionsBuilder = new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter(_options.Value.StatusTopic);

            //added all topics
            AddTopicsToMqttClient(msgOptionsBuilder);

            var msgOptions = msgOptionsBuilder.Build();

            return await _mqttClient.SubscribeAsync(msgOptions, cancellationToken);
        }

        private void AddTopicsToMqttClient(MqttClientSubscribeOptionsBuilder options)
        {
            foreach (var topic in _sensorTopics) 
            {
                options.WithTopicFilter(topics => topics.WithTopic(topic.Topic));
            }
        }

        public void SubscribeApplicationMessageReceived()
        {
            ArgumentNullException.ThrowIfNull(_mqttClient);

            _mqttClient.ApplicationMessageReceivedAsync += ApplicationMessageReceivedAsync;
        }

        private async Task ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
        {
            string topics = args.ApplicationMessage.Topic;
            string payload = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);

            Match matchedTopic = _regex.Match(topics);

            if (matchedTopic.Success)
            {
                string sensorId = matchedTopic.Groups[0].Value;

                var responseData = JsonSerializer.Deserialize<MqttResponseModel>(payload);

                //send to db queue
                await _mqttHistorianService.AddToQueue(responseData);

                _logger.LogInformation($"sensorId : {responseData.Name} values : {responseData.Value} TimeStamp : {responseData.TimeStamp}");
            }
            else if (topics.Equals(_options.Value.StatusTopic))
            {
                string sensorId = topics;
                _logger.LogInformation($"sensorId : {sensorId} values : {payload}");
            }
        }
    }
}
