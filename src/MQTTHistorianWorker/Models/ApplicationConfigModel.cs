using MQTTHistorianWorker.Services.MQTTHistorianService.Models;
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
        public string? UniqueClientID { get; set; } = "MQTT Historian Service";
        public string? StatusTopic { get; set; }
        public List<SensorsTopics> SensorsTopics { get; set; } = new();
        public MqttServerConfigModel? MqttServer { get; set; }
        public HistorianDataBaseConnectionModel? ConnectionConfiguration { get; set; }
    }
}
