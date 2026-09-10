using ManufacturingSystem.Sensors;
using ManufacturingSystem.WorkflowEngine.Stages;
using ManufacturingSystem.WorkflowEngine.Workflows;
using Microsoft.Extensions.Logging;
using Moq;

namespace ManufacturingSystem.Test.Modules.Workflow;

public sealed class IWorkflowProcessorTests
{
    private readonly Mock<IWorkflow> _workflowMock;
    private readonly Mock<IStageProcessor> _stageProcessorMock;
    private readonly IWorkflowProcessor _workflowProcessor;
    private readonly SensorSnapshot _snapshot;

    public IWorkflowProcessorTests()
    {
        _workflowMock = new Mock<IWorkflow>();
        _stageProcessorMock = new Mock<IStageProcessor>();
        var loggerMock = new Mock<ILogger<WorkflowProcessor>>();

        _workflowMock
            .SetupGet(workflow => workflow.Id)
            .Returns(WorkflowId.Production);
       
        loggerMock
            .Setup(logger => logger.IsEnabled(LogLevel.Information))
            .Returns(false);

        _workflowProcessor = new WorkflowProcessor(
            [_workflowMock.Object],
            _stageProcessorMock.Object,
            loggerMock.Object);
        _snapshot = new SensorSnapshot(
            new DateTimeOffset(2026, 9, 11, 0, 0, 0, TimeSpan.Zero),
            new Dictionary<SensorType, double>
            {
                [SensorType.Temperature] = 25,
                [SensorType.Pressure] = 75
            });
    }

    [Fact]
    public async Task Execute_WhenWorkflowSelectsStages_ExecutesSelectedStages()
    {
        // Arrange
        StageId[] selectedStageIds = [StageId.Stage1, StageId.Stage2];
        _workflowMock
            .Setup(workflow => workflow.SelectStages(_snapshot))
            .Returns(selectedStageIds);
        _stageProcessorMock
            .Setup(processor => processor.Execute(
                selectedStageIds,
                CancellationToken.None));

        // Act
        var result = await Record.ExceptionAsync(
            () => _workflowProcessor.Execute(
                WorkflowId.Production,
                _snapshot,
                CancellationToken.None));

        // Assert
        Assert.Null(result);
        _stageProcessorMock.Verify(
            processor => processor.Execute(selectedStageIds, CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task Execute_WhenWorkflowSelectsNoStages_DoesNotExecuteStageProcessor()
    {
        // Arrange
        _workflowMock
            .Setup(workflow => workflow.SelectStages(_snapshot))
            .Returns([]);

        // Act
        var result = await Record.ExceptionAsync(
            () => _workflowProcessor.Execute(
                WorkflowId.Production,
                _snapshot,
                CancellationToken.None));

        // Assert
        Assert.Null(result);
        _stageProcessorMock.Verify(
            processor => processor.Execute(
                It.IsAny<IReadOnlyCollection<StageId>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
