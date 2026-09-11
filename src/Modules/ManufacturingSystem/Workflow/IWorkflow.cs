using ManufacturingSystem.Modules.Sensors;
using ManufacturingSystem.Modules.Stages;

namespace ManufacturingSystem.Modules.Workflow;

public interface IWorkflow
{
    WorkflowId Id { get; }
    IReadOnlyCollection<StageId> SelectStages(SensorSnapshot snapshot);
}
