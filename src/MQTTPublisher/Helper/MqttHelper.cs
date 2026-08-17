using MQTTnet.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTPublisher.Helper
{
    public static class MqttHelper
    {
        public static MqttQualityOfServiceLevel GetMqttQualityOfService(this int no)
        {
            switch (no)
            {
                case 0:
                    return MqttQualityOfServiceLevel.AtMostOnce;
                case 1:
                    return MqttQualityOfServiceLevel.AtLeastOnce;
                case 2:
                    return MqttQualityOfServiceLevel.ExactlyOnce;
                default:
                    return MqttQualityOfServiceLevel.AtMostOnce;
            }
        }
    }
}
