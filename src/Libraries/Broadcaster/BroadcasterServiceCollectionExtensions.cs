using Microsoft.Extensions.DependencyInjection;

namespace Broadcaster;

public static class BroadcasterServiceCollectionExtensions
{
    public static IServiceCollection AddBroadcaster(this IServiceCollection services)
    {
        services.AddSingleton<IBroadcaster, Broadcaster>();

        return services;
    }
}
