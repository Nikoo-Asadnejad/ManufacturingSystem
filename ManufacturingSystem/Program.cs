using ManufacturingSystem;
using ManufacturingSystem.Modules.Sensors;
using ManufacturingSystem.Modules.ResourceManagement;
using ManufacturingSystem.Modules.Stages;
using ManufacturingSystem.Modules.Workflow;
using ManufacturingSystem.Modules.Workflow.Rules;
using System.Threading.Tasks.Dataflow;
using InternalQueue;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton(
    new BroadcastBlock<SensorMeasurement>(measurement => measurement));
builder.Services.AddSingleton<ISensor, TemperatureSensor>();
builder.Services.AddSingleton<ISensor, PressureSensor>();
builder.Services.AddSingleton<IEventBus, EventBus>();

builder.Services.AddSingleton<IRule, TemperatureAboveFivePressureBelowFiftyRule>();
builder.Services.AddSingleton<IRule, TemperatureAboveTenPressureBelowHundredRule>();
builder.Services.AddSingleton<IRule, TemperatureAboveTwentyPressureBelowHundredRule>();
builder.Services.AddSingleton<IWorkflow, ProductionWorkflow>();
builder.Services.AddSingleton<IWorkflowProcessor, WorkflowProcessor>();

builder.Services.AddSingleton<IStage, Stage1>();
builder.Services.AddSingleton<IStage, Stage2>();
builder.Services.AddSingleton<IStage, Stage3>();
builder.Services.AddSingleton<IStageProcessor, StageProcessor>();

builder.Services.AddSingleton(new Resource("R_A"));
builder.Services.AddSingleton(new Resource("R_B"));
builder.Services.AddSingleton(new Resource("R_C"));
builder.Services.AddSingleton<IResourceCoordinator, ResourceCoordinator>();

builder.Services.AddHostedService<SensorSnapshotGenerator>();
builder.Services.AddHostedService<SensorSnapshotConsumer>();

var host = builder.Build();
host.Run();
