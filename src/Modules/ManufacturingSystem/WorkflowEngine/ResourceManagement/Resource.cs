namespace ManufacturingSystem.WorkflowEngine.ResourceManagement;

internal sealed class Resource : IResource
{
    public Resource(string id)
    {
        Id = id;
        _state = (int)ResourceState.Idle;
    }
    public string Id { get; }
    
    private int _state;
    public ResourceState State => (ResourceState)Volatile.Read(ref _state);

    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task<bool> Acquire(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (State != ResourceState.Idle)
            {
                return false;
            }

            MarkBusy();
            return true;
        }
        catch (Exception e)
        {
            //log exception and mark error
            return false;
        }
        finally
        {
            if(State != ResourceState.Busy)
            {
                 _semaphore.Release();
            }
        }
    }

    public async Task<bool> Release(CancellationToken cancellationToken)
    {
        var markedIde = false;
        try
        {
            markedIde = TryMarkIdle();
            return true;
        }
        catch (Exception e)
        {
            //log exception and mark error
            return false;
        }
        finally
        {
            if(markedIde)
            {
                _semaphore.Release();
            }
        }
    }
    
    private void MarkBusy()
    {
        Interlocked.Exchange(ref _state, (int)ResourceState.Busy);
    }
    
    private bool TryMarkIdle()
    {
       return Interlocked.CompareExchange(ref _state,
          value : (int)ResourceState.Idle , 
          comparand: (int)ResourceState.Busy) == (int)ResourceState.Busy;
    }
    
}
