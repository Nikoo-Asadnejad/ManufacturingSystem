namespace ManufacturingSystem.WorkflowEngine.ResourceManagement;

public interface IResourceCoordinator
{
    Task<IResource[]> AcquireAsync(
        IEnumerable<string> resourceIds,
        CancellationToken cancellationToken = default);

    Task ReleaseAsync(IEnumerable<IResource> resources ,
        CancellationToken cancellationToken = default);
}
