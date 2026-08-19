using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTHistorianWorker.Models
{
    public class ApplicationConfigModel
    {
        public static string MqttServerConfigSection { get; set; } = "ApplicationConfig";
        public MqttServerConfigModel? MqttServer { get; set; }
    }
}
