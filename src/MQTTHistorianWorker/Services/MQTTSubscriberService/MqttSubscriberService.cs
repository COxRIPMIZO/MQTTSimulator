using MQTTnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTHistorianWorker.Services.MQTTSubscriberService
{
    public class MqttSubscriberService : IMqttSubscriberService
    {
        readonly ILogger<MqttSubscriberService> _logger;
        readonly IMqttClient _mqttClient;

        public MqttSubscriberService(ILogger<MqttSubscriberService> logger, IMqttClient mqttClient)
        {
            _logger = logger;
            _mqttClient = mqttClient;
        }
        public Task<MqttClientConnectResult> ConfigureMqttClientAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
