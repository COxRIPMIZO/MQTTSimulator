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
using System.Threading.Tasks;

namespace MQTTHistorianWorker.Services.MQTTHistorianService.Repository
{
    public class MqttHistorianService : IMqttHistorianService
    {
        private ConcurrentQueue<MqttResponseModel> InsertDataQueue = new();
        private readonly HistorianDataBaseConnectionModel? _connection;
        private readonly ILogger _logger;
        //private DataTable PayloadDataTable = new();

        //prevent multiple thread to access db insert function at the same time
        private readonly SemaphoreSlim _dbLock = new(1, 1);

        public MqttHistorianService(ILogger<MqttHistorianService> logger, IOptions<ApplicationConfigModel> options)
        {
            _logger = logger;
            _connection = options.Value.ConnectionConfiguration;

            ////create datatable
            //CreateDataTable();
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

        public Task AddToQueue(MqttResponseModel data, CancellationToken cancellationToken = default)
        {
            try
            {
                InsertDataQueue.Enqueue(data);

                _logger.LogInformation("Data inserted into queue for insert.");

                if (InsertDataQueue.Count >= _connection.BulkInsertCount)
                {
                    _ = QueueProcessing(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fatal Error during Queue Insert: {ex.Message}");
            }

            return Task.CompletedTask;
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
            //block multiple thred to insert data at the same time
            if(!await _dbLock.WaitAsync(0))
                return;

            try
            {
                //create datatable
                using DataTable dt = CreateDataTable();

                int processedCount = 0;

                while (InsertDataQueue.TryDequeue(out var result) && processedCount < _connection.BulkInsertCount)
                {
                    AddRecordsIntoDataTable(dt,result);
                    processedCount++;
                }

                if (dt.Rows.Count > 0)
                {
                    await SentToDataTable(dt);
                    _logger.LogInformation($"Successfully bulk inserted {dt.Rows.Count} rows into SQL Server.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fatal Error during DB Bulk Insert: {ex.Message}");
            }
            finally
            {
                _dbLock.Release();
            }
        }
    }
}
