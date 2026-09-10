using ManufacturingSystem.Modules.ResourceManagement;

namespace ManufacturingSystem.Modules.Stages;

internal interface IStage
{
    StageId Id { get; }

    IReadOnlyList<string> RequiredResourceIds { get; }

    void Execute(
        IReadOnlyList<IResource> resources,
        CancellationToken cancellationToken = default);
}