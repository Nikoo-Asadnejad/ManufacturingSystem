
namespace ManufacturingSystem.Modules.Sensors;

internal sealed class TemperatureSensor : ISensor
{
    public SensorType SensorType => SensorType.Temperature;
    public ValueTask<double> ReadAsync(CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(Random.Shared.NextDouble() * 30);
    }
}
