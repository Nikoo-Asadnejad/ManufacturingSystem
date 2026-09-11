using System.Threading.Tasks.Dataflow;

namespace Broadcaster;

public interface IBroadcaster
{
    Task BroadcastAsync(IBroadcastEvent message, CancellationToken cancellationToken = default);

    void Subscribe<TEvent>(ITargetBlock<IBroadcastEvent> consumer) where TEvent : IBroadcastEvent;
}
