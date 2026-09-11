using ManufacturingSystem.Modules.Sensors;

namespace ManufacturingSystem.Modules.Workflow;

public interface IWorkflowProcessor
{
    void Execute(
        WorkflowId workflowId,
        SensorSnapshot snapshot,
        CancellationToken cancellationToken = default);
}
