namespace ManufacturingSystem.Modules.ResourceManagement;

public interface IResource
{
    string Id { get; }
    ResourceState State { get; }
}