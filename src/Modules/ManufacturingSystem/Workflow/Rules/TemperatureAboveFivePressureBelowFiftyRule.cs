using ManufacturingSystem.Modules.Sensors;
using ManufacturingSystem.Modules.Stages;

namespace ManufacturingSystem.Modules.Workflow.Rules;

internal sealed class TemperatureAboveFivePressureBelowFiftyRule : IRule
{
    public bool Matches(SensorSnapshot snapshot) =>
        snapshot.TryGetValue(SensorType.Temperature, out var temperature) &&
        snapshot.TryGetValue(SensorType.Pressure, out var pressure) &&
        temperature > 5.0 &&
        pressure < 50.0;

    public IReadOnlyCollection<StageId> GetStages(SensorSnapshot snapshot) =>
        [StageId.Stage3, StageId.Stage2];
}
