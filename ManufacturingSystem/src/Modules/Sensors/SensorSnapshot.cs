using System.Collections.ObjectModel;

namespace ManufacturingSystem.Modules.Sensors;

public sealed record SensorSnapshot
{
    public SensorSnapshot(
        long sequence,
        DateTimeOffset timestamp,
        IReadOnlyDictionary<SensorType, double> values)
    {
        Sequence = ValidateSequence(sequence);
        Timestamp = timestamp;
        Values = CreateValues(values);
    }

    public long Sequence { get; }

    public DateTimeOffset Timestamp { get; }

    public IReadOnlyDictionary<SensorType, double> Values { get; }

    public bool TryGetValue(SensorType sensorType, out double value) =>
        Values.TryGetValue(sensorType, out value);

    private static IReadOnlyDictionary<SensorType, double> CreateValues(
        IReadOnlyDictionary<SensorType, double> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        return new ReadOnlyDictionary<SensorType, double>(
            new Dictionary<SensorType, double>(values));
    }

    private static long ValidateSequence(long sequence)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(sequence);
        return sequence;
    }
}
