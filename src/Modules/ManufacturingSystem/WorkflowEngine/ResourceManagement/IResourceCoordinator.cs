namespace ManufacturingSystem.WorkflowEngine.ResourceManagement;

public interface IResourceCoordinator
{
    Task<IResource[]> AcquireAsync(
        IEnumerable<string> resourceIds,
        CancellationToken cancellationToken = default);

    void Release(
        IEnumerable<IResource> resources);
}
