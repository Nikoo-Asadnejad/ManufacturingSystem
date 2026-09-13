using ManufacturingSystem.WorkflowEngine.ResourceManagement;

namespace ManufacturingSystem.WorkflowEngine.Stages;

internal sealed class StageProcessor(
    IEnumerable<IStage> stages,
    IResourceCoordinator resourceCoordinator,
    ILogger<StageProcessor> logger) : IStageProcessor
{
    private readonly IReadOnlyDictionary<StageId, IStage> _stages =
        stages.ToDictionary(stage => stage.Id);

    public async Task Execute(
        IReadOnlyCollection<StageId> stageIds,
        CancellationToken cancellationToken = default)
    {
        var selectedStages = GetSelectedStages(stageIds);

        await Parallel.ForEachAsync(
            selectedStages,
            CreateParallelOptions(selectedStages.Length, cancellationToken),
            async (stage, stageCancellationToken) =>
                await ExecuteStage(stage, stageCancellationToken));
    }
    
    private async Task ExecuteStage(
        IStage stage,
        CancellationToken cancellationToken)
    {
        IResource[] resources = [];

        try
        {
            // waits until all required resource are acquired, only if resource is error will return empty
            resources = await resourceCoordinator.AcquireAsync(
                stage.RequiredResourceIds,
                cancellationToken);

            if (resources.Length < stage.RequiredResourceIds.Count)
            {
                logger.LogCritical("Stage :{StageId} could not be executed because of resource allocation. see logs for allocation errors." ,stage.Id);
                return;
            }

            stage.Execute(resources, cancellationToken);

            logger.LogInformation("Stage {Stage} completed.", stage.Id);
        }
        catch (Exception exception)
        {
            logger.LogCritical(exception, "Stage {StageId} failed." , stage.Id);
            throw; // operation should not continue.
        }
        finally
        {
             resourceCoordinator.Release(resources);
        }
    }

    private IStage? GetStage(StageId stageId)
    { 
        _stages.TryGetValue(stageId, out var stage);
        return stage;
    }

    private IStage[] GetSelectedStages(IReadOnlyCollection<StageId> stageIds)
    {
        var selectedStages = stageIds
            .Distinct()
            .Select(GetStage)
            .Where(s=> s != null)
            .Select(s=> s!)
            .ToArray();
        return selectedStages;
    }

    private static ParallelOptions CreateParallelOptions(
        int stageCount,
        CancellationToken cancellationToken) =>
        new()
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = Math.Min(Math.Max(1, stageCount),
                 Environment.ProcessorCount),
        };

}
