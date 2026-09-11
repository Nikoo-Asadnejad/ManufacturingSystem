using Broadcaster;
using InternalQueue;
using ManufacturingSystem.Modules.Sensors;
using ManufacturingSystem.Modules.Workflow;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddBroadcaster()
    .AddInternalQueue()
    .AddSensorModule()
    .AddWorkflowModule();

var host = builder.Build();
host.Run();
