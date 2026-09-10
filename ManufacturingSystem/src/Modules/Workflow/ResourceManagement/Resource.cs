using System.Security.Cryptography;
using System.Text;

namespace ManufacturingSystem.Modules.ResourceManagement;

internal sealed class Resource : IResource
{
    public Resource(string id)
    {
        Id = id;
    }
    public string Id { get; }
    
    private int _state;

    public ResourceState State => (ResourceState)Volatile.Read(ref _state);

    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public bool Acquire(CancellationToken cancellationToken)
    {
        _semaphore.Wait(cancellationToken);

        if (Volatile.Read(ref _state) != (int)ResourceState.Idle)
        {
            return false;
        }

        try
        {
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

    public bool Release(CancellationToken cancellationToken)
    {
        _semaphore.Wait();
      
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
