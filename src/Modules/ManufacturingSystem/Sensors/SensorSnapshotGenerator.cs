using System.Threading.Tasks.Dataflow;
using Broadcaster;
using InternalQueue;

namespace ManufacturingSystem.Sensors;

internal sealed class SensorSnapshotGenerator : BackgroundService
{
    private static readonly TimeSpan GenerationInterval = TimeSpan.FromMilliseconds(100);
    private readonly BufferBlock<IBroadcastEvent> _temperatureMeasurementConsumer = new(GetOption());
    private readonly BufferBlock<IBroadcastEvent> _pressureMeasurementConsumer = new(GetOption());
    private readonly IEventBus _eventBus;
    private readonly ILogger<SensorSnapshotGenerator> _logger;

    public SensorSnapshotGenerator(
        IBroadcaster broadcaster,
        IEventBus eventBus,
        ILogger<SensorSnapshotGenerator> logger)
    {
        _eventBus = eventBus;
        _logger = logger;

        broadcaster.Subscribe<SensorMeasurement>(_temperatureMeasurementConsumer);
        broadcaster.Subscribe<SensorMeasurement>(_pressureMeasurementConsumer );
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var temperatureMeasurement = _temperatureMeasurementConsumer.ReceiveAsync(stoppingToken);
            var pressureMeasurement = _pressureMeasurementConsumer.ReceiveAsync(stoppingToken);

            var measurements = await Task.WhenAll(
                temperatureMeasurement,
                pressureMeasurement);

            var snapshot = new SensorSnapshot(
                DateTimeOffset.UtcNow,
                measurements.Select(m => (SensorMeasurement)m)
                    .DistinctBy(m=> m.SensorType)
                    .ToDictionary(m=> m.SensorType ,m=> m.Value));
            
            await _eventBus.PublishAsync(snapshot, stoppingToken);

            _logger.LogInformation(
                "Generated sensor snapshot at {Timestamp}.",
                snapshot.Timestamp);

            await Task.Delay(GenerationInterval, stoppingToken);
        }
    }

    private static DataflowBlockOptions GetOption()
    {
        return new DataflowBlockOptions()
        {
            BoundedCapacity = 1
        };
    } 
}
