using ManufacturingSystem.WorkflowEngine.ResourceManagement;
using ManufacturingSystem.WorkflowEngine.Rules;
using ManufacturingSystem.WorkflowEngine.Stages;
using ManufacturingSystem.WorkflowEngine.Workflows;

namespace ManufacturingSystem.WorkflowEngine;

public static class WorkflowServiceCollectionExtensions
{
    public static IServiceCollection AddWorkflowModule(this IServiceCollection services)
    {
        services.AddSingleton<IRule, TemperatureAboveFivePressureBelowFiftyRule>();
        services.AddSingleton<IRule, TemperatureAboveTenPressureBelowHundredRule>();
        services.AddSingleton<IRule, TemperatureAboveTwentyPressureBelowHundredRule>();
        services.AddSingleton<IWorkflow, ProductionWorkflow>();
        services.AddSingleton<IWorkflowProcessor, WorkflowProcessor>();

        services.AddSingleton<IStage, Stage1>();
        services.AddSingleton<IStage, Stage2>();
        services.AddSingleton<IStage, Stage3>();
        services.AddSingleton<IStageProcessor, StageProcessor>();

        services.AddSingleton(new Resource("R_A"));
        services.AddSingleton(new Resource("R_B"));
        services.AddSingleton(new Resource("R_C"));
        services.AddSingleton<IResourceCoordinator, ResourceCoordinator>();

        services.AddHostedService<SensorSnapshotConsumer>();

        return services;
    }
}
