using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTHistorianWorker.Services.MQTTHistorianService.Models
{
    public class HistorianDataBaseConnectionModel
    {
        public string? ConnectionString { get; set; }
        public int BulkInsertCount { get; set; }
        public int BufferCount { get; set; }
        public int RetryCount { get; set; }
        public int WaitingTime { get; set; }
    }
}
