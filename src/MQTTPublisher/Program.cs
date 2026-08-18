using MQTTPublisher;
using MQTTPublisher.Helper;
using MQTTPublisher.Models;
using MQTTnet;
using MQTTPublisher.Services.MQTTPublisherService;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

#region COnfigured Service

// Add configuration for MQTT settings
builder.Services.Configure<EdgePoints>(builder.Configuration.GetSection("EdgePoints"));

builder.Services.AddTransient<GetRandomData>();

builder.Services.AddSingleton<IMqttPublish, MqttPublish>();

builder.Services.AddSingleton<IMqttClient>(provider =>
{
    var mqttfactory = new MqttClientFactory();
    return mqttfactory.CreateMqttClient();
});

#endregion

var host = builder.Build();
host.Run();
