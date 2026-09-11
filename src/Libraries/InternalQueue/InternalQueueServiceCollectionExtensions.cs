using Microsoft.Extensions.DependencyInjection;

namespace InternalQueue;

public static class InternalQueueServiceCollectionExtensions
{
    public static IServiceCollection AddInternalQueue(this IServiceCollection services)
    {
        services.AddSingleton<IEventBus, EventBus>();

        return services;
    }
}
