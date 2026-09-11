using System.Threading.Tasks.Dataflow;

namespace Broadcaster;

public class Broadcaster : IBroadcaster
{
    private readonly BroadcastBlock<IBroadcastEvent> _broadcastBlock;

    public Broadcaster()
    {
        _broadcastBlock = new BroadcastBlock<IBroadcastEvent>(evt => evt);
    }

    public async Task BroadcastAsync(IBroadcastEvent message, CancellationToken cancellationToken = default)
    {
        await _broadcastBlock.SendAsync(message, cancellationToken);
    }

    public void Subscribe<TEvent>(ITargetBlock<IBroadcastEvent> consumer, Predicate<TEvent> filter) where TEvent : IBroadcastEvent
    {
        _broadcastBlock.LinkTo(consumer, message => message is TEvent tEvent && filter(tEvent));
    }
}
