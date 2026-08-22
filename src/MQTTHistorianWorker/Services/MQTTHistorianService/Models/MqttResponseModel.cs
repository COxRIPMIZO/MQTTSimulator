using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTHistorianWorker.Services.MQTTHistorianService.Models
{
    public class MqttResponseModel
    {
        public string? Name { get; set; }
        public int Value { get; set; }
        public DateTime? TimeStamp { get; set; }
    }
}
