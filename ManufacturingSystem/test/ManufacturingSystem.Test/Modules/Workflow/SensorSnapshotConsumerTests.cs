using System.Threading.Tasks.Dataflow;
using InternalQueue;
using ManufacturingSystem.Modules.Sensors;
using ManufacturingSystem.Modules.Workflow;
using Microsoft.Extensions.Logging;
using Moq;

namespace ManufacturingSystem.Test.Modules.Workflow;

public sealed class SensorSnapshotConsumerTests
{
    private readonly BroadcastBlock<SensorMeasurement> _broadcaster;
    private readonly SensorSnapshotGenerator _generator;
    private readonly SensorSnapshotConsumer _consumer;
    private readonly TaskCompletionSource<SensorSnapshot> _processedSnapshot;

    public SensorSnapshotConsumerTests()
    {
        _broadcaster = new BroadcastBlock<SensorMeasurement>(measurement => measurement);
        var eventBus = new EventBus();
        var workflowMock = new Mock<IWorkflow>(MockBehavior.Strict);
        var workflowProcessorMock = new Mock<IWorkflowProcessor>(MockBehavior.Strict);
        var generatorLoggerMock = new Mock<ILogger<SensorSnapshotGenerator>>();
        var consumerLoggerMock = new Mock<ILogger<SensorSnapshotConsumer>>();
        _processedSnapshot = new TaskCompletionSource<SensorSnapshot>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        workflowMock
            .SetupGet(workflow => workflow.Id)
            .Returns(WorkflowId.Production);
        workflowProcessorMock
            .Setup(processor => processor.Execute(
                WorkflowId.Production,
                It.IsAny<SensorSnapshot>(),
                It.IsAny<CancellationToken>()))
            .Callback<WorkflowId, SensorSnapshot, CancellationToken>(
                (_, snapshot, _) => _processedSnapshot.TrySetResult(snapshot));

        _generator = new SensorSnapshotGenerator(
            _broadcaster,
            eventBus,
            generatorLoggerMock.Object);
        _consumer = new SensorSnapshotConsumer(
            eventBus,
            [workflowMock.Object],
            workflowProcessorMock.Object,
            consumerLoggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMeasurementsAreBroadcast_ProcessesQueuedSnapshot()
    {
        // Arrange
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        await _generator.StartAsync(cancellation.Token);
        await _consumer.StartAsync(cancellation.Token);

        // Act
        await _broadcaster.SendAsync(
            new SensorMeasurement(SensorType.Temperature, 25),
            cancellation.Token);
        await _broadcaster.SendAsync(
            new SensorMeasurement(SensorType.Pressure, 75),
            cancellation.Token);
        var result = await _processedSnapshot.Task.WaitAsync(cancellation.Token);

        await _generator.StopAsync(CancellationToken.None);
        await _consumer.StopAsync(CancellationToken.None);

        // Assert
        Assert.Equal(1, result.Sequence);
        Assert.Equal(25, result.Values[SensorType.Temperature]);
        Assert.Equal(75, result.Values[SensorType.Pressure]);
    }
}
