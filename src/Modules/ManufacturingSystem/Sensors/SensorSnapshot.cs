using InternalQueue;

namespace ManufacturingSystem.Sensors;

public sealed record SensorSnapshot : IInternalEvent
{
    public SensorSnapshot(
        DateTimeOffset timestamp,
        IReadOnlyDictionary<SensorType, double> values)
    {
        Timestamp = timestamp;
        Values = values.ToDictionary();
    }

    public DateTimeOffset Timestamp { get; }

    public IReadOnlyDictionary<SensorType, double> Values { get; }

    public bool TryGetValue(SensorType sensorType, out double value) =>
        Values.TryGetValue(sensorType, out value);

}
