namespace ManufacturingSystem.WorkflowEngine.ResourceManagement;

public interface IResource
{
    string Id { get; }
    ResourceState State { get; }
    Task<bool> Acquire(CancellationToken cancellationToken);
    bool Release();
}