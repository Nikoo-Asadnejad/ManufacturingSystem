using ManufacturingSystem.Sensors;

namespace ManufacturingSystem.WorkflowEngine.Workflows;

public interface IWorkflowProcessor
{
    Task Execute(
        WorkflowId workflowId,
        SensorSnapshot snapshot,
        CancellationToken cancellationToken = default);
}
