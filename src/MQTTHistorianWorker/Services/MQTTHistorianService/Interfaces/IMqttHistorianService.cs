using MQTTHistorianWorker.Services.MQTTHistorianService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTHistorianWorker.Services.MQTTHistorianService.Interfaces
{
    public interface IMqttHistorianService
    {
        //Task InsertDataAsync(CancellationToken cancellationToken = default);
        ValueTask DiconnectAsync();
        Task AddToQueue(MqttResponseModel data, CancellationToken cancellationToken = default);
    }
}
