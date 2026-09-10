namespace ManufacturingSystem.Modules.ResourceManagement;

public interface IResourceCoordinator
{
    bool Acquire(
        IEnumerable<string> resourceIds,
        CancellationToken cancellationToken = default);
}
