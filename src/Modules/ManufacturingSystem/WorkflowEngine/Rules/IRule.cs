using ManufacturingSystem.Sensors;
using ManufacturingSystem.WorkflowEngine.Stages;

namespace ManufacturingSystem.WorkflowEngine.Rules;

internal interface IRule
{
    bool Matches(SensorSnapshot snapshot);

    IReadOnlyCollection<StageId> GetStages(SensorSnapshot snapshot);
}
