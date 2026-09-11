using Microsoft.Extensions.DependencyInjection;

namespace ManufacturingSystem.Modules.Sensors;

public static class SensorServiceCollectionExtensions
{
    public static IServiceCollection AddSensorModule(this IServiceCollection services)
    {
        services.AddSingleton<ISensor, TemperatureSensor>();
        services.AddSingleton<ISensor, PressureSensor>();
        services.AddHostedService<SensorSnapshotGenerator>();

        return services;
    }
}
