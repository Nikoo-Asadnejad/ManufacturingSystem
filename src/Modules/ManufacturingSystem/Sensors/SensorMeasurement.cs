using Broadcaster;

namespace ManufacturingSystem.Modules.Sensors;

public sealed record SensorMeasurement(
    SensorType SensorType,
    double Value) : IBroadcastEvent;
