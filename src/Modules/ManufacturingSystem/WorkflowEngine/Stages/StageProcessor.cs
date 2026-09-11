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
        var selectedStages = stageIds
            .Distinct()
            .Select(GetStage)
            .Where(s=> s != null)
            .Select(s=> s!)
            .ToArray();

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
            // wait until all required resource are acquired, only if resource is error will return empty
            resources = await resourceCoordinator.AcquireAsync(
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
            await resourceCoordinator.ReleaseAsync(resources);
        }
    }

    private IStage? GetStage(StageId stageId)
    { 
        _stages.TryGetValue(stageId, out var stage);
        return stage;
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
