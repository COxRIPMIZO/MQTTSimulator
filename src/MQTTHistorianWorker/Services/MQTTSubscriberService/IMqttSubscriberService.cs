using MQTTnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTHistorianWorker.Services.MQTTSubscriberService
{
    public interface IMqttSubscriberService
    {
        Task DisconnectAsync(CancellationToken cancellationToken = default);
        Task<MqttClientConnectResult> ConfigureMqttClientAsync(CancellationToken cancellationToken = default);
        Task<MqttClientSubscribeResult> SubscribeMessagesAsync(CancellationToken cancellationToken = default);
    }
}
