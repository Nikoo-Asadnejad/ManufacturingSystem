namespace ManufacturingSystem.Modules.Stages;

public interface IStageProcessor
{
    Task Execute(
        IReadOnlyCollection<StageId> stageIds,
        CancellationToken cancellationToken = default);
}
