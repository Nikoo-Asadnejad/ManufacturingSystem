using Broadcaster;

namespace ManufacturingSystem.Modules.Sensors;
internal sealed class TemperatureSensor(IBroadcaster broadcaster) : ISensor
{
    public SensorType SensorType => SensorType.Temperature;

    public async ValueTask<double> ReadAsync(CancellationToken cancellationToken)
    {
        var value = Random.Shared.NextDouble() * 30;

        await broadcaster.BroadcastAsync(
            new SensorMeasurement(SensorType, value),
            cancellationToken);

        return value;
    }
}
