using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTHistorianWorker.Models
{
    public class MqttServerConfigModel
    {
        public string? BaseAddress { get; set; }
        public int Port { get; set; }
        public bool WillRetain { get; set; }
        public int QualityOfService { get; set; }
    }
}
