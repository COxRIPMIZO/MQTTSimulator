using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using MQTTHistorianWorker.Models;
using MQTTHistorianWorker.Services.MQTTHistorianService.Interfaces;
using MQTTHistorianWorker.Services.MQTTHistorianService.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MQTTHistorianWorker.Services.MQTTHistorianService.Repository
{
    public class MqttHistorianService : IMqttHistorianService
    {
        private Channel<MqttResponseModel> _channel;
        private readonly HistorianDataBaseConnectionModel? _connection;
        private readonly ILogger _logger;
        private Task _consumer;
        private readonly CancellationTokenSource _cts = new();

        //prevent multiple thread to access db insert function at the same time
        //private readonly SemaphoreSlim _dbLock = new(1, 1);

        public MqttHistorianService(ILogger<MqttHistorianService> logger, IOptions<ApplicationConfigModel> options)
        {
            _logger = logger;
            _connection = options.Value.ConnectionConfiguration;

            ////create datatable
            //CreateDataTable();
            CreateAndConfigChannel();
        }

        private void CreateAndConfigChannel()
        {
            var chnlOption = new BoundedChannelOptions(_connection.BufferCount)
            {
                Capacity = _connection.BulkInsertCount,
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false
            };

            _channel = Channel.CreateBounded<MqttResponseModel>(chnlOption);

            _consumer = Task.Run(() => QueueProcessing());
        }

        private DataTable CreateDataTable()
        {
            DataTable dataTable = new DataTable();

            string[] colums = new string[] {"Name","Value","TimeStamp" };
            foreach (var colum in colums)
            {
                DataColumn cols = new DataColumn(colum,typeof(string));
                dataTable.Columns.Add(cols);
            }

            return dataTable;
        }

        public async Task AddToQueue(MqttResponseModel data, CancellationToken cancellationToken = default)
        {
            try
            {
                //InsertDataQueue.Enqueue(data);

                //_logger.LogInformation("Data inserted into queue for insert.");

                //if (InsertDataQueue.Count >= _connection.BulkInsertCount)
                //{
                //    _ = QueueProcessing(cancellationToken);
                //}

                await _channel.Writer.WriteAsync(data,cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fatal Error during Queue Insert: {ex.Message}");
            }
        }

        private void AddRecordsIntoDataTable( DataTable dataTable,MqttResponseModel? responseModel)
        {
            string[] rowData = new string[]
            {
                responseModel.Name,
                responseModel.Value.ToString(),
                responseModel.TimeStamp.ToString()
            };

            dataTable.Rows.Add(rowData);
        }

        private async Task ProcessingData(List<MqttResponseModel> responseModels)
        {
            using DataTable data = CreateDataTable();

            //add records
            foreach (var responseModel in responseModels) 
            {
                AddRecordsIntoDataTable(data,responseModel);
            }

            //send data
            await SentToDataTable(data);
        }

        private async Task<int> SentToDataTable(DataTable data)
        {
            using (IDbConnection conn = new SqlConnection(_connection.ConnectionString))
            {
                var parameter = new
                {
                    MqttDataType = data.AsTableValuedParameter("MqttDataType")
                };

                return await conn.ExecuteAsync("SP_InsertDataIntoRegisterValue",parameter,commandType:CommandType.StoredProcedure);
            }
        }

        private async Task QueueProcessing(CancellationToken cancellationToken = default)
        {
            var data = new List<MqttResponseModel>();

            try
            {
               while(await _channel.Reader.WaitToReadAsync(_cts.Token))
                {
                    while (_channel.Reader.TryRead(out var item))
                    {
                        data.Add(item);

                        if(data.Count >= _connection.BulkInsertCount)
                        {
                            await ProcessingData(data);
                            data.Clear();
                        }
                    }

                    //send remaing data
                    if(data.Count > 0)
                    {
                        await ProcessingData(data);
                        data.Clear();
                    }
                }
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception ex)
            {
                _logger.LogError($"Fatal Error during DB Bulk Insert: {ex.Message}");
            }
        }

        public async ValueTask DiconnectAsync()
        {
            //notified the channel the to task is compelte 
            _channel.Writer.Complete();

            try
            {
                await _consumer;
            }
            catch (Exception)
            {

            }

            _cts.Cancel();
            _cts.Dispose();
        }
    }
}
