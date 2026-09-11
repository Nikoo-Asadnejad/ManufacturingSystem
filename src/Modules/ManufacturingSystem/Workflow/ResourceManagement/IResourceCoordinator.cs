namespace ManufacturingSystem.Modules.ResourceManagement;

public interface IResourceCoordinator
{
    IResource[] Acquire(
        IEnumerable<string> resourceIds,
        CancellationToken cancellationToken = default);

    void Release(IEnumerable<IResource> resources);
}
