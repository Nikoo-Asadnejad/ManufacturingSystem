using System.Threading.Tasks.Dataflow;
using InternalQueue;

namespace ManufacturingSystem.Modules.Sensors;

internal sealed class SensorSnapshotGenerator : BackgroundService
{
    private static readonly TimeSpan GenerationInterval = TimeSpan.FromMilliseconds(100);
    private readonly BufferBlock<SensorMeasurement> _temperatureConsumer = new();
    private readonly BufferBlock<SensorMeasurement> _pressureConsumer = new();
    private readonly IEventBus _eventBus;
    private readonly ILogger<SensorSnapshotGenerator> _logger;
    private long _sequence;

    public SensorSnapshotGenerator(
        BroadcastBlock<SensorMeasurement> broadcaster,
        IEventBus eventBus,
        ILogger<SensorSnapshotGenerator> logger)
    {
        _eventBus = eventBus;
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
                DateTimeOffset.UtcNow,
                measurements.ToDictionary(
                    measurement => measurement.SensorType,
                    measurement => measurement.Value));

            await _eventBus.PublishAsync(snapshot, stoppingToken);

            _logger.LogInformation(
                "Generated sensor snapshot at {Timestamp}.",
                snapshot.Timestamp);

            // check there would be no issue when measurments are not created at same time
            await Task.Delay(GenerationInterval, stoppingToken);
        }
    }
}
