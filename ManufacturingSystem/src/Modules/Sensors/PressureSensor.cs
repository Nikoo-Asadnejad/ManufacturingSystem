namespace ManufacturingSystem.Modules.Sensors;

internal sealed class PressureSensor : ISensor
{
    public SensorType SensorType => SensorType.Pressure;
    public ValueTask<double> ReadAsync(CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(Random.Shared.NextDouble() * 120);
    }
}
