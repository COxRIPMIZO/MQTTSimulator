using Microsoft.Extensions.Options;
using MQTTHistorianWorker;
using MQTTHistorianWorker.Models;
using MQTTHistorianWorker.Services.MQTTHistorianService.Interfaces;
using MQTTHistorianWorker.Services.MQTTHistorianService.Repository;
using MQTTHistorianWorker.Services.MQTTSubscriberService;
using MQTTnet;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

#region Adding dependency

builder.Services.Configure<ApplicationConfigModel>(builder.Configuration.GetSection(ApplicationConfigModel.MqttServerConfigSection));

builder.Services.AddSingleton<IMqttSubscriberService, MqttSubscriberService>();
builder.Services.AddTransient<IMqttHistorianService, MqttHistorianService>();


builder.Services.AddSingleton<IMqttClient>(provider => 
{
    var factory = new MqttClientFactory();
    return factory.CreateMqttClient();
});

#endregion

var host = builder.Build();
host.Run();
