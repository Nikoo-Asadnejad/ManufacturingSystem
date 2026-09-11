using ManufacturingSystem.WorkflowEngine.ResourceManagement;
using ManufacturingSystem.WorkflowEngine.Stages;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ManufacturingSystem.Test.Modules.Workflow.Stages;

public sealed class StageProcessorTests
{
    [Fact]
    public async Task Execute_WhenStageIsKnown_AcquiresExecutesAndReleasesResources()
    {
        // Arrange
        string[] requiredResourceIds = ["R_A"];
        var resource = MockResource("R_A");
        var stage = MockStage(StageId.Stage1, requiredResourceIds);
        var resourceCoordinator = MockResourceCoordinator([resource]);
        var processor = CreateStageProcessor([stage.Object], resourceCoordinator.Object);

        // Act
        await processor.Execute([StageId.Stage1], CancellationToken.None);

        // Assert
        resourceCoordinator.VerifyAll();
        stage.VerifyAll();
    }

    [Fact]
    public async Task Execute_WhenStageIdsContainDuplicates_ExecutesStageOnce()
    {
        // Arrange
        var stage = MockStage(StageId.Stage1, []);
        var resourceCoordinator = MockResourceCoordinator([]);
        var processor = CreateStageProcessor([stage.Object], resourceCoordinator.Object);

        // Act
        await processor.Execute(
            [StageId.Stage1, StageId.Stage1],
            CancellationToken.None);

        // Assert
        resourceCoordinator.Verify(
            coordinator => coordinator.AcquireAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        
        resourceCoordinator.Verify(
            coordinator => coordinator.ReleaseAsync(
                It.IsAny<IEnumerable<IResource>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        
        stage.Verify(
            configuredStage => configuredStage.Execute(
                It.IsAny<IReadOnlyList<IResource>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Execute_WhenNotAllResourcesAreAcquired_DoesNotExecuteStageAndReleasesAcquiredResources()
    {
        // Arrange
        string[] requiredResourceIds = ["R_A", "R_B"];
        var acquiredResource = MockResource("R_A");
        var stage = MockStage(StageId.Stage1, requiredResourceIds);
        var resourceCoordinator = MockResourceCoordinator([acquiredResource]);
        var processor = CreateStageProcessor([stage.Object], resourceCoordinator.Object);

        // Act
        await processor.Execute([StageId.Stage1], CancellationToken.None);

        // Assert
        stage.Verify(
            configuredStage => configuredStage.Execute(
                It.IsAny<IReadOnlyList<IResource>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        
        resourceCoordinator.Verify(rc=> rc.ReleaseAsync(
            It.IsAny<IReadOnlyList<IResource>>(),
            It.IsAny<CancellationToken>()) , Times.Once);
    }


    private static Mock<IStage> MockStage(
        StageId id,
        IReadOnlyList<string> requiredResourceIds)
    {
        var stage = new Mock<IStage>();

        stage.SetupGet(configuredStage => configuredStage.Id).Returns(id);

        stage
            .SetupGet(configuredStage => configuredStage.RequiredResourceIds)
            .Returns(requiredResourceIds);

        stage.Setup(configuredStage => configuredStage.Execute(
            It.IsAny<IReadOnlyList<IResource>>(),
            It.IsAny<CancellationToken>()));

        return stage;
    }

    private static StageProcessor CreateStageProcessor(
        IEnumerable<IStage> stages,
        IResourceCoordinator resourceCoordinator)
    {
        return new(
            stages,
            resourceCoordinator,
            NullLogger<StageProcessor>.Instance);
    }

    private static Mock<IResourceCoordinator> MockResourceCoordinator(
        IEnumerable<IResource> resources)
    {
        var coordinator = new Mock<IResourceCoordinator>();

        coordinator
            .Setup(configuredCoordinator => configuredCoordinator.AcquireAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(resources.ToArray());

        coordinator
            .Setup(configuredCoordinator => configuredCoordinator.ReleaseAsync(
                It.IsAny<IEnumerable<IResource>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return coordinator;
    }

    private static IResource MockResource(string id)
    {
        var resource = new Mock<IResource>();
        resource.SetupGet(configuredResource => configuredResource.Id).Returns(id);

        return resource.Object;
    }
}
