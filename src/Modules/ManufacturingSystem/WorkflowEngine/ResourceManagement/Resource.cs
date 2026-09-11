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
            //log exception
            return false;
        }
        finally
        {
           _semaphore.Release();
        }
    }

    public async Task<bool> Release(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
      
        try
        {
            MarkIdle();
            return true;
        }
        catch (Exception e)
        {
            //log exception
            return false;
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    private void MarkBusy()
    {
        Interlocked.Exchange(ref _state, (int)ResourceState.Busy);
    }
    
    private void MarkIdle()
    {
        Interlocked.Exchange(ref _state, (int)ResourceState.Idle);
    }
    
}
