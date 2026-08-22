using MQTTnet;
using MQTTPublisher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTPublisher.Services.MQTTPublisherService
{
    public interface IMqttPublish
    {
        Task<MqttClientConnectResult> ConfigureMqttClientAsync(CancellationToken cancellationToken = default);
        Task<MqttClientPublishResult> PublishStatusMessageAsync(bool isConnect,CancellationToken cancellationToken = default);
        Task<MqttClientPublishResult> PublishMessageAsync(EdgePointsDetailModel edgePointsDetails, CancellationToken cancellationToken = default);
    }
}
