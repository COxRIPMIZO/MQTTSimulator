using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTPublisher.Models
{
    public class EdgePoints
    {
        public string BrokerAddress { get; set; } = string.Empty;
        public int Port { get; set; }
        public List<EdgePointsDetails> Sensors { get; set; } = new();
        public string StatusTopic { get; set; } = string.Empty;
        public string StatusOfflineMessage { get; set; } = string.Empty;
        public string StatusOnlineMessage { get; set; } = string.Empty;
        public bool WillRetain { get; set; }
        public int QualityOfService { get; set; }
    }
}
