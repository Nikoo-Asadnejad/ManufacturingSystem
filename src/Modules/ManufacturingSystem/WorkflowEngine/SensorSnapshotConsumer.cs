using InternalQueue;
using ManufacturingSystem.Sensors;
using ManufacturingSystem.WorkflowEngine.Workflows;

namespace ManufacturingSystem.WorkflowEngine;

internal sealed class SensorSnapshotConsumer : BackgroundService
{
    private readonly IEventBus _eventBus;
    private readonly WorkflowId[] _workflowIds;
    private readonly IWorkflowProcessor _workflowProcessor;
    private readonly ILogger<SensorSnapshotConsumer> _logger;

    public SensorSnapshotConsumer(
        IEventBus eventBus,
        IEnumerable<IWorkflow> workflows,
        IWorkflowProcessor workflowProcessor,
        ILogger<SensorSnapshotConsumer> logger)
    {
        _eventBus = eventBus;
        _workflowIds = workflows
            .Select(workflow => workflow.Id)
            .Distinct()
            .ToArray();
        _workflowProcessor = workflowProcessor;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var internalEvent in _eventBus.ReadAllAsync(stoppingToken))
        {
            if (internalEvent is not SensorSnapshot snapshot || snapshot is null)
            {
                //skip this event.
                continue;
            }


            _logger.LogInformation($"Snap shot {snapshot.Timestamp} recieved .");
          
            foreach (var workflowId in _workflowIds)
            {
                try
                {
                    await _workflowProcessor.Execute(workflowId, snapshot, stoppingToken);
                }
                catch (Exception exception)
                {
                    _logger.LogCritical(
                        exception,
                        "Workflow {WorkflowId} failed to process sensor snapshot created at {Timestamp}.",
                        workflowId,
                        snapshot.Timestamp);

                    //continue to next workflow.
                }
            }
        }
    }
}
