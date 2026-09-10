using System.Threading.Channels;

namespace ManufacturingSystem.Libraries.InternalQueue;

public sealed class EventBus : IEventBus
{
    private readonly Channel<IInternalEvent> _queue = Channel.CreateUnbounded<IInternalEvent>();

    public ValueTask PublishAsync(
        IInternalEvent internalEvent,
        CancellationToken cancellationToken = default) =>
        _queue.Writer.WriteAsync(internalEvent, cancellationToken);

    public IAsyncEnumerable<IInternalEvent> ReadAllAsync(
        CancellationToken cancellationToken = default) =>
        _queue.Reader.ReadAllAsync(cancellationToken);
}
