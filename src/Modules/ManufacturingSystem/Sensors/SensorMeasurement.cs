using Broadcaster;

namespace ManufacturingSystem.Sensors;

public sealed record SensorMeasurement(
    SensorType SensorType,
    double Value) : IBroadcastEvent;
