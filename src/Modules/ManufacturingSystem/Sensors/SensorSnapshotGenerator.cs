using System.Threading.Tasks.Dataflow;
using Broadcaster;
using InternalQueue;

namespace ManufacturingSystem.Modules.Sensors;

internal sealed class SensorSnapshotGenerator : BackgroundService
{
    private static readonly TimeSpan GenerationInterval = TimeSpan.FromMilliseconds(100);
    private readonly BufferBlock<IBroadcastEvent> _measurementConsumer = new();
    private readonly HashSet<SensorType> _sensorTypes;
    private readonly IEventBus _eventBus;
    private readonly ILogger<SensorSnapshotGenerator> _logger;

    public SensorSnapshotGenerator(
        IBroadcaster broadcaster,
        IEnumerable<ISensor> sensors,
        IEventBus eventBus,
        ILogger<SensorSnapshotGenerator> logger)
    {
        _sensorTypes = sensors
            .Select(sensor => sensor.SensorType)
            .ToHashSet();
        _eventBus = eventBus;
        _logger = logger;

        broadcaster.Subscribe<SensorMeasurement>(_measurementConsumer);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var measurements = new Dictionary<SensorType, double>();

            while (measurements.Count < _sensorTypes.Count)
            {
                var broadcastEvent = await _measurementConsumer.ReceiveAsync(stoppingToken);

                if (broadcastEvent is SensorMeasurement measurement)
                {
                    measurements.TryAdd(measurement.SensorType, measurement.Value);
                }
            }

            var snapshot = new SensorSnapshot(
                DateTimeOffset.UtcNow,
                measurements);
            
            await _eventBus.PublishAsync(snapshot, stoppingToken);

            _logger.LogInformation(
                "Generated sensor snapshot at {Timestamp}.",
                snapshot.Timestamp);

            // check there would be no issue when measurments are not created at same time
            await Task.Delay(GenerationInterval, stoppingToken);
        }
    }
}
