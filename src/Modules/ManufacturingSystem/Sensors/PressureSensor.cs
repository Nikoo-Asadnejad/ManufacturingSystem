using System.Threading.Tasks.Dataflow;

namespace ManufacturingSystem.Modules.Sensors;

internal sealed class PressureSensor(
    BroadcastBlock<SensorMeasurement> broadcaster) : ISensor
{
    public SensorType SensorType => SensorType.Pressure;

    public async ValueTask<double> ReadAsync(CancellationToken cancellationToken)
    {
        var value = Random.Shared.NextDouble() * 120;

        await broadcaster.SendAsync(
            new SensorMeasurement(SensorType, value),
            cancellationToken);

        return value;
    }
}
