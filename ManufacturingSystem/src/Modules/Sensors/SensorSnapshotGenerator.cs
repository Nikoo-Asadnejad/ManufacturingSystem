using System.Threading.Tasks.Dataflow;

namespace ManufacturingSystem.Modules.Sensors;

internal sealed class SensorSnapshotGenerator : BackgroundService
{
    private static readonly TimeSpan GenerationInterval = TimeSpan.FromMilliseconds(100);
    private readonly ISensor[] _sensors;
    private readonly BufferBlock<SensorMeasurement> _temperatureConsumer = new();
    private readonly BufferBlock<SensorMeasurement> _pressureConsumer = new();
    private readonly ILogger<SensorSnapshotGenerator> _logger;
    private long _sequence;

    public SensorSnapshotGenerator(
        IEnumerable<ISensor> sensors,
        BroadcastBlock<SensorMeasurement> broadcaster,
        ILogger<SensorSnapshotGenerator> logger)
    {
        _sensors = sensors.ToArray();
        _logger = logger;

        broadcaster.LinkTo(
            _temperatureConsumer,
            measurement => measurement.SensorType == SensorType.Temperature);

        broadcaster.LinkTo(
            _pressureConsumer,
            measurement => measurement.SensorType == SensorType.Pressure);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var temperatureMeasurement = _temperatureConsumer.ReceiveAsync(stoppingToken);
            var pressureMeasurement = _pressureConsumer.ReceiveAsync(stoppingToken);

            var measurements = await Task.WhenAll(
                temperatureMeasurement,
                pressureMeasurement);

            var snapshot = new SensorSnapshot(
                ++_sequence,
                DateTimeOffset.UtcNow,
                measurements.ToDictionary(
                    measurement => measurement.SensorType,
                    measurement => measurement.Value));

            _logger.LogInformation(
                "Generated sensor snapshot {Sequence} at {Timestamp}.",
                snapshot.Sequence,
                snapshot.Timestamp);

            // check there would be no issue when measurments are not created at same time
            await Task.Delay(GenerationInterval, stoppingToken);
        }
    }
}
