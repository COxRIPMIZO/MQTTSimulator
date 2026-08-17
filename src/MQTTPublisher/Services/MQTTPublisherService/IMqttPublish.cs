using MQTTnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTPublisher.Services.MQTTPublisherService
{
    public interface IMqttPublish
    {
        Task ConfigureMqttClient();
        Task PublishConnectMessage();
        Task PublishDisconnectMessage();
    }
}
