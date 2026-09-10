using ManufacturingSystem.Modules.ResourceManagement;

namespace ManufacturingSystem.Modules.Stages;

internal sealed class StageProcessor(
    IEnumerable<IStage> stages,
    IResourceCoordinator resourceCoordinator,
    ILogger<StageProcessor> logger) : IStageProcessor
{
    private readonly IReadOnlyDictionary<StageId, IStage> _stages =
        stages.ToDictionary(stage => stage.Id);

    public void Execute(
        IReadOnlyCollection<StageId> stageIds,
        CancellationToken cancellationToken = default)
    {
        var selectedStages = stageIds
            .Distinct()
            .Select(GetStage)
            .ToArray();

        Parallel.ForEach(
            selectedStages,
            CreateParallelOptions(selectedStages.Length, cancellationToken),
            stage => ExecuteStage(stage, cancellationToken));
    }

    private void ExecuteStage(
        IStage stage,
        CancellationToken cancellationToken)
    {
        IResource[] resources = [];

        try
        {
            // wait until all required resource are aquired, only if resource is error will return empty
            resources = resourceCoordinator.Acquire(
                stage.RequiredResourceIds,
                cancellationToken);

            if (resources.Length < stage.RequiredResourceIds.Count)
            {
                logger.LogCritical($"Stage :{stage.Id} could not be executed because of resource allocation. see logs for allocation errors.");
                return;
            }

            stage.Execute(resources, cancellationToken);

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Stage {Stage} completed.", stage.Id);
            }
        }
        catch (Exception exception)
        {
            logger.LogCritical(exception, $"Stage {stage.Id} failed.");
            throw; // if one stage of a workflow fail we won't continue.
        }
        finally
        {
            resourceCoordinator.Release(resources);
        }
    }

    private IStage GetStage(StageId stageId)
    {
        return _stages.TryGetValue(stageId, out var stage)
            ? stage
            : throw new KeyNotFoundException($"Stage '{stageId}' is not registered.");
    }

    private static ParallelOptions CreateParallelOptions(
        int stageCount,
        CancellationToken cancellationToken) =>
        new()
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = Math.Min(
                Math.Max(1, stageCount),
                Environment.ProcessorCount)
        };

}
