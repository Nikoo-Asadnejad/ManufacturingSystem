namespace ManufacturingSystem.WorkflowEngine.ResourceManagement;

public interface IResource
{
    string Id { get; }
    ResourceState State { get; }
}