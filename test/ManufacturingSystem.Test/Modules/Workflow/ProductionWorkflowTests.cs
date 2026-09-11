using ManufacturingSystem.Sensors;
using ManufacturingSystem.WorkflowEngine.Rules;
using ManufacturingSystem.WorkflowEngine.Stages;
using ManufacturingSystem.WorkflowEngine.Workflows;
using Moq;

namespace ManufacturingSystem.Test.Modules.Workflow;

public sealed class ProductionWorkflowTests
{
    private readonly Mock<IRule> _firstRuleMock;
    private readonly Mock<IRule> _secondRuleMock;
    private readonly ProductionWorkflow _workflow;
    private readonly SensorSnapshot _snapshot;

    public ProductionWorkflowTests()
    {
        _firstRuleMock = new Mock<IRule>();
        _secondRuleMock = new Mock<IRule>();
        _workflow = new ProductionWorkflow([_firstRuleMock.Object, _secondRuleMock.Object]);
        _snapshot = new SensorSnapshot(
            new DateTimeOffset(2026, 9, 11, 0, 0, 0, TimeSpan.Zero),
            new Dictionary<SensorType, double>
            {
                [SensorType.Temperature] = 25,
                [SensorType.Pressure] = 75
            });
    }

    [Fact]
    public void Id_ReturnsProduction()
    {
        // Arrange
        var expected = WorkflowId.Production;

        // Act
        var result = _workflow.Id;

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void SelectStages_WhenNoRuleMatches_ReturnsEmptyCollection()
    {
        // Arrange
        _firstRuleMock
            .Setup(rule => rule.Matches(_snapshot))
            .Returns(false);
        _secondRuleMock
            .Setup(rule => rule.Matches(_snapshot))
            .Returns(false);

        // Act
        var result = _workflow.SelectStages(_snapshot);

        // Assert
        Assert.Empty(result);
        _firstRuleMock.Verify(rule => rule.Matches(_snapshot), Times.Once);
        _secondRuleMock.Verify(rule => rule.Matches(_snapshot), Times.Once);
    }

    [Fact]
    public void SelectStages_WhenRulesMatch_ReturnsDistinctOrderedStages()
    {
        // Arrange
        _firstRuleMock
            .Setup(rule => rule.Matches(_snapshot))
            .Returns(true);
        _firstRuleMock
            .Setup(rule => rule.GetStages(_snapshot))
            .Returns(new[] { StageId.Stage3, StageId.Stage1 });
        _secondRuleMock
            .Setup(rule => rule.Matches(_snapshot))
            .Returns(true);
        _secondRuleMock
            .Setup(rule => rule.GetStages(_snapshot))
            .Returns(new[] { StageId.Stage2, StageId.Stage1 });

        // Act
        var result = _workflow.SelectStages(_snapshot);

        // Assert
        Assert.Equal(
            new[] { StageId.Stage1, StageId.Stage2, StageId.Stage3 },
            result);
        _firstRuleMock.Verify(rule => rule.GetStages(_snapshot), Times.Once);
        _secondRuleMock.Verify(rule => rule.GetStages(_snapshot), Times.Once);
    }
}
