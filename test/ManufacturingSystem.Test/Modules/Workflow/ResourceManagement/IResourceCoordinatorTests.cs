using ManufacturingSystem.WorkflowEngine.ResourceManagement;
using Microsoft.Extensions.Logging;
using Moq;

namespace ManufacturingSystem.Test.Modules.Workflow.ResourceManagement;

public sealed class IResourceCoordinatorTests
{
    private readonly Resource _resourceA;
    private readonly Resource _resourceB;
    private readonly IResourceCoordinator _resourceCoordinator;

    public IResourceCoordinatorTests()
    {
        _resourceA = new Resource("R_A");
        _resourceB = new Resource("R_B");

        var loggerMock = new Mock<ILogger<ResourceCoordinator>>(MockBehavior.Strict);
        _resourceCoordinator = new ResourceCoordinator(
            [_resourceB, _resourceA],
            loggerMock.Object);
    }

    [Fact]
    public async Task Acquire_ReturnsDistinctResourcesInIdOrder()
    {
        // Arrange
        string[] resourceIds = ["R_B", "R_A", "R_B"];

        // Act
        var result = await _resourceCoordinator.AcquireAsync(resourceIds, CancellationToken.None);

        // Assert
        Assert.Equal(new[] { "R_A", "R_B" }, result.Select(resource => resource.Id));
        Assert.All(result, resource => Assert.Equal(ResourceState.Busy, resource.State));
    }

    [Fact]
    public async Task Release_ReleasesResourcesAcquiredByCoordinator()
    {
        // Arrange
        var resources = await _resourceCoordinator.AcquireAsync(
            new[] { "R_A", "R_B" },
            CancellationToken.None);

        // Act
        var result = await Record.ExceptionAsync(
            () => _resourceCoordinator.ReleaseAsync(resources));

        // Assert
        Assert.Null(result);
        Assert.All(resources, resource => Assert.Equal(ResourceState.Idle, resource.State));
    }

}
