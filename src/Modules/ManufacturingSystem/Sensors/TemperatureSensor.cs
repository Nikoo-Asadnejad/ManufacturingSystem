using System.Threading.Tasks.Dataflow;

namespace ManufacturingSystem.Modules.Sensors;

internal sealed class TemperatureSensor(
    BroadcastBlock<SensorMeasurement> broadcaster) : ISensor
{
    public SensorType SensorType => SensorType.Temperature;

    public async ValueTask<double> ReadAsync(CancellationToken cancellationToken)
    {
        var value = Random.Shared.NextDouble() * 30;

        await broadcaster.SendAsync(
            new SensorMeasurement(SensorType, value),
            cancellationToken);

        return value;
    }
}
