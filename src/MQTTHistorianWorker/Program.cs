using Microsoft.Extensions.Options;
using MQTTHistorianWorker;
using MQTTHistorianWorker.Models;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

#region Adding dependency

builder.Services.Configure<ApplicationConfigModel>(builder.Configuration.GetSection(ApplicationConfigModel.MqttServerConfigSection));

#endregion

var host = builder.Build();
host.Run();
