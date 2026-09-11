using Broadcaster;

namespace ManufacturingSystem.Sensors;

internal sealed class PressureSensor(IBroadcaster broadcaster) : ISensor
{
    public SensorType SensorType => SensorType.Pressure;

    public async ValueTask<double> ReadAsync(CancellationToken cancellationToken)
    {
        var value = Random.Shared.NextDouble() * 120;

        await broadcaster.BroadcastAsync(
            new SensorMeasurement(SensorType, value),
            cancellationToken);

        return value;
    }
}
