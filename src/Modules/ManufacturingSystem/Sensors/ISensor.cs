namespace ManufacturingSystem.Modules.Sensors;

public interface ISensor
{
    SensorType SensorType { get; }
    ValueTask<double> ReadAsync(CancellationToken cancellationToken);
}