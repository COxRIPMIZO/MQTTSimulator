using MQTTPublisher;
using MQTTPublisher.Helper;
using MQTTPublisher.Models;
using MQTTnet;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

// Add configuration for MQTT settings
builder.Services.Configure<EdgePoints>(builder.Configuration.GetSection("EdgePoints"));

//configurs service
builder.Services.AddTransient<GetRandomData>();

builder.Services.AddSingleton<IMqttClient>(provider => 
{
    var mqttfactory = new MqttClientFactory();
    return mqttfactory.CreateMqttClient();
});


var host = builder.Build();
host.Run();
