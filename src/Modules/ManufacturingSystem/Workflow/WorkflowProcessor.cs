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
        if (_workflows.TryGetValue(workflowId, out var workflow))
        {
            return;
        }
        
        var stageIds = SelectAndLogStages(workflow, snapshot);
        
        if (stageIds.Count <= 0)
        {
            logger.LogWarning("No Stages were found to run for workflowId {wId}", workflow.Id);
        }
        
        stageProcessor.Execute(stageIds, cancellationToken);
    }
    
    private IReadOnlyCollection<StageId> SelectAndLogStages(
        IWorkflow workflow,
        SensorSnapshot snapshot)
    {
        var stageIds = workflow.SelectStages(snapshot);
        return stageIds;
    }

}
