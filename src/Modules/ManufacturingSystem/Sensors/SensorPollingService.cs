namespace ManufacturingSystem.Sensors;

/// <summary>
/// This service has built for development and test process to simulate the data.
/// </summary>
/// <param name="sensors"></param>
internal sealed class SensorPollingService(
    IEnumerable<ISensor> sensors) : BackgroundService
{
    private static readonly TimeSpan PollingInterval =
        TimeSpan.FromMilliseconds(100);

    private readonly ISensor[] _sensors = sensors.ToArray();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var sensor in _sensors)
            {
                await sensor.ReadAsync(stoppingToken);
            }

            await Task.Delay(PollingInterval, stoppingToken);
        }
    }
}
