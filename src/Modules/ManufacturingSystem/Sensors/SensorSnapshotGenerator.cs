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

        broadcaster.Subscribe<SensorMeasurement>(_temperatureMeasurementConsumer ,
            m=> m.SensorType == SensorType.Temperature);
        broadcaster.Subscribe<SensorMeasurement>(_pressureMeasurementConsumer ,
            m=> m.SensorType == SensorType.Pressure);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var temperatureMeasurement = _temperatureMeasurementConsumer.ReceiveAsync(stoppingToken);
            var pressureMeasurement = _pressureMeasurementConsumer.ReceiveAsync(stoppingToken);

             await Task.WhenAll(
                temperatureMeasurement,
                pressureMeasurement);
            
            var temperature = await temperatureMeasurement as SensorMeasurement;
            var pressure = await pressureMeasurement as SensorMeasurement;

            if (pressure is null || temperature is null)
            {
                _logger.LogError($"Broken Snapshot : temp :{temperature?.Value} , pressure : {pressure?.Value}.");
                continue;
            }

            var measurementsMap = new Dictionary<SensorType, double>
            {
                [temperature!.SensorType] = temperature.Value,
                [pressure!.SensorType] = pressure.Value
            };

            var snapshot = new SensorSnapshot(
                DateTimeOffset.UtcNow,
                measurementsMap);
            
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
