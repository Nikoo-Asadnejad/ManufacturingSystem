using ManufacturingSystem.Modules.Sensors;
using ManufacturingSystem.Modules.Stages;

namespace ManufacturingSystem.Modules.Workflow;

internal sealed class WorkflowProcessor(
    IEnumerable<IWorkflow> workflows,
    IStageProcessor stageProcessor,
    ILogger<WorkflowProcessor> logger) : IWorkflowProcessor
{
    private readonly Dictionary<WorkflowId, IWorkflow> _workflows =
        workflows.ToDictionary(workflow => workflow.Id);

    public void Execute(
        WorkflowId workflowId,
        SensorSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        var workflow = GetWorkflow(workflowId);

        try
        {
            var stageIds = SelectAndLogStages(workflow, snapshot);
            if (stageIds.Count > 0)
            {
                stageProcessor.Execute(stageIds, cancellationToken);
            }
            else
            {
                logger.LogWarning("No Stages were found to run for workflowId {wId}", workflow.Id);
            }
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Workflow {Workflow} failed for sensor snapshot .",
                workflow.GetType().Name);
        }
    }

    private IWorkflow GetWorkflow(WorkflowId workflowId)
    {
        return _workflows.TryGetValue(workflowId, out var workflow)
            ? workflow
            : throw new KeyNotFoundException($"Workflow '{workflowId}' is not registered.");
    }

    private IReadOnlyCollection<StageId> SelectAndLogStages(
        IWorkflow workflow,
        SensorSnapshot snapshot)
    {
        var stageIds = workflow.SelectStages(snapshot);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Workflow {Workflow} selected stages {Stages} for sensor snapshot.",
                workflow.GetType().Name,
                stageIds);
        }

        return stageIds;
    }

}
