using System.Threading.Tasks.Dataflow;

namespace Broadcaster;

public interface IBroadcaster
{
    Task BroadcastAsync(IBroadcastEvent message, CancellationToken cancellationToken = default);

    void Subscribe<TEvent>(ITargetBlock<IBroadcastEvent> consumer , Predicate<TEvent> filter) where TEvent : IBroadcastEvent;
}
