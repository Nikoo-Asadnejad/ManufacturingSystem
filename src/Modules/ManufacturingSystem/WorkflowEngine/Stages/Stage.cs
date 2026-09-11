using ManufacturingSystem.WorkflowEngine.ResourceManagement;

namespace ManufacturingSystem.WorkflowEngine.Stages;

internal abstract class Stage(
    StageId id,
    IReadOnlyList<string> requiredResourceIds) : IStage
{
    public StageId Id { get; } = id;

    public IReadOnlyList<string> RequiredResourceIds { get; } = requiredResourceIds;

    public void Execute(
        IReadOnlyList<IResource> resources,
        CancellationToken cancellationToken = default)
    {
    }
    
}