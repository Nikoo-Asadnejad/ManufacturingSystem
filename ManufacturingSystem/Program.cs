using ManufacturingSystem;
using ManufacturingSystem.Modules.Sensors;
using System.Threading.Tasks.Dataflow;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton(
    new BroadcastBlock<SensorMeasurement>(measurement => measurement));
builder.Services.AddSingleton<ISensor, TemperatureSensor>();
builder.Services.AddSingleton<ISensor, PressureSensor>();
builder.Services.AddHostedService<SensorSnapshotGenerator>();

var host = builder.Build();
host.Run();
