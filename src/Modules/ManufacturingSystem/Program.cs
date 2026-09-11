using Broadcaster;
using InternalQueue;
using ManufacturingSystem.Sensors;
using ManufacturingSystem.WorkflowEngine;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddBroadcaster()
    .AddInternalQueue()
    .AddSensorModule()
    .AddWorkflowModule();

var host = builder.Build();
host.Run();
