using ManufacturingSystem.Sensors;
using ManufacturingSystem.WorkflowEngine.Rules;
using ManufacturingSystem.WorkflowEngine.Stages;

namespace ManufacturingSystem.Test.Modules.Workflow.Rules;

public sealed class IRuleTests
{
    [Fact]
    public void TemperatureAboveFivePressureBelowFiftyRule_MatchesAndReturnsStages()
    {
        // Arrange
        IRule rule = new TemperatureAboveFivePressureBelowFiftyRule();
        var matchingSnapshot = CreateSnapshot(temperature: 6, pressure: 49);

        // Act and assert
        Assert.True(rule.Matches(matchingSnapshot));
        Assert.False(rule.Matches(CreateSnapshot(temperature: 5, pressure: 49)));
        Assert.False(rule.Matches(CreateSnapshot(temperature: 6, pressure: 50)));
        Assert.Equal(
            [StageId.Stage3, StageId.Stage2],
            rule.GetStages(matchingSnapshot));
    }

    [Fact]
    public void TemperatureAboveTenPressureBelowHundredRule_MatchesAndReturnsStages()
    {
        // Arrange
        IRule rule = new TemperatureAboveTenPressureBelowHundredRule();
        var matchingSnapshot = CreateSnapshot(temperature: 11, pressure: 99);

        // Act and assert
        Assert.True(rule.Matches(matchingSnapshot));
        Assert.False(rule.Matches(CreateSnapshot(temperature: 10, pressure: 99)));
        Assert.False(rule.Matches(CreateSnapshot(temperature: 11, pressure: 100)));
        Assert.Equal(
            [StageId.Stage1, StageId.Stage2],
            rule.GetStages(matchingSnapshot));
    }

    [Fact]
    public void TemperatureAboveTwentyPressureBelowHundredRule_MatchesAndReturnsStages()
    {
        // Arrange
        IRule rule = new TemperatureAboveTwentyPressureBelowHundredRule();
        var matchingSnapshot = CreateSnapshot(temperature: 21, pressure: 99);

        // Act and assert
        Assert.True(rule.Matches(matchingSnapshot));
        Assert.False(rule.Matches(CreateSnapshot(temperature: 20, pressure: 99)));
        Assert.False(rule.Matches(CreateSnapshot(temperature: 21, pressure: 100)));
        Assert.Equal(
            [StageId.Stage1, StageId.Stage3],
            rule.GetStages(matchingSnapshot));
    }

    private static SensorSnapshot CreateSnapshot(
        double temperature,
        double pressure) =>
        new(
            new DateTimeOffset(2026, 9, 11, 0, 0, 0, TimeSpan.Zero),
            new Dictionary<SensorType, double>
            {
                [SensorType.Temperature] = temperature,
                [SensorType.Pressure] = pressure
            });
}
