using ManufacturingSystem.Modules.Sensors;
using ManufacturingSystem.Modules.Stages;

namespace ManufacturingSystem.Modules.Workflow.Rules;

internal sealed class TemperatureAboveTenPressureBelowHundredRule : IRule
{
    public bool Matches(SensorSnapshot snapshot) =>
        snapshot.TryGetValue(SensorType.Temperature, out var temperature) &&
        snapshot.TryGetValue(SensorType.Pressure, out var pressure) &&
        temperature > 10.0 &&
        pressure < 100.0;

    public IReadOnlyCollection<StageId> GetStages(SensorSnapshot snapshot) =>
        [StageId.Stage1, StageId.Stage2];
}
