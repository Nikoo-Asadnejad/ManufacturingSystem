namespace ManufacturingSystem.Sensors;

public interface ISensor
{
    SensorType SensorType { get; }
    ValueTask<double> ReadAsync(CancellationToken cancellationToken);
}