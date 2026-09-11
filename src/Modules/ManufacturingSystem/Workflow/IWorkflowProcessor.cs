using ManufacturingSystem.Modules.Sensors;

namespace ManufacturingSystem.Modules.Workflow;

public interface IWorkflowProcessor
{
    Task Execute(
        WorkflowId workflowId,
        SensorSnapshot snapshot,
        CancellationToken cancellationToken = default);
}
