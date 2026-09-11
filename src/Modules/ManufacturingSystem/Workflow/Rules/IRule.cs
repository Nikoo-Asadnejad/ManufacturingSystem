using ManufacturingSystem.Modules.Sensors;
using ManufacturingSystem.Modules.Stages;

namespace ManufacturingSystem.Modules.Workflow.Rules;

internal interface IRule
{
    bool Matches(SensorSnapshot snapshot);

    IReadOnlyCollection<StageId> GetStages(SensorSnapshot snapshot);
}
