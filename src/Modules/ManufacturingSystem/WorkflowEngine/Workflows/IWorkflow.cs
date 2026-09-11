using ManufacturingSystem.Sensors;
using ManufacturingSystem.WorkflowEngine.Stages;

namespace ManufacturingSystem.WorkflowEngine.Workflows;

public interface IWorkflow
{
    WorkflowId Id { get; }
    IReadOnlyCollection<StageId> SelectStages(SensorSnapshot snapshot);
}
