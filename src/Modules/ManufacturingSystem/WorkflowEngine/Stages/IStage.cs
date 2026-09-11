using ManufacturingSystem.WorkflowEngine.ResourceManagement;

namespace ManufacturingSystem.WorkflowEngine.Stages;

internal interface IStage
{
    StageId Id { get; }

    IReadOnlyList<string> RequiredResourceIds { get; }

    void Execute(
        IReadOnlyList<IResource> resources,
        CancellationToken cancellationToken = default);
}