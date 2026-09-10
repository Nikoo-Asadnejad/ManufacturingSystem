namespace ManufacturingSystem.Libraries.InternalQueue;

public interface IEventBus
{
    ValueTask PublishAsync(
        IInternalEvent internalEvent,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<IInternalEvent> ReadAllAsync(
        CancellationToken cancellationToken = default);
}
