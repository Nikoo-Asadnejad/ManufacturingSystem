namespace ManufacturingSystem.Modules.Stages;

public interface IStageProcessor
{
    void Execute(
        IReadOnlyCollection<StageId> stageIds,
        CancellationToken cancellationToken = default);
}
