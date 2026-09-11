using ManufacturingSystem.Modules.ResourceManagement;
using Microsoft.Extensions.Logging;
using Moq;

namespace ManufacturingSystem.Test.Modules.ResourceManagement;

public sealed class IResourceCoordinatorTests
{
    private readonly Resource _resourceA;
    private readonly Resource _resourceB;
    private readonly IResourceCoordinator _resourceCoordinator;

    public IResourceCoordinatorTests()
    {
        _resourceA = new Resource("R_A");
        _resourceB = new Resource("R_B");
        _resourceA.Release(CancellationToken.None);
        _resourceB.Release(CancellationToken.None);

        var loggerMock = new Mock<ILogger<ResourceCoordinator>>(MockBehavior.Strict);
        _resourceCoordinator = new ResourceCoordinator(
            [_resourceB, _resourceA],
            loggerMock.Object);
    }

    [Fact]
    public void Acquire_ReturnsDistinctResourcesInIdOrder()
    {
        // Arrange
        string[] resourceIds = ["R_B", "R_A", "R_B"];

        // Act
        var result = _resourceCoordinator.Acquire(resourceIds, CancellationToken.None);

        // Assert
        Assert.Equal(new[] { "R_A", "R_B" }, result.Select(resource => resource.Id));
        Assert.All(result, resource => Assert.Equal(ResourceState.Busy, resource.State));
    }

    [Fact]
    public void Release_ReleasesResourcesAcquiredByCoordinator()
    {
        // Arrange
        var resources = _resourceCoordinator.Acquire(
            new[] { "R_A", "R_B" },
            CancellationToken.None);

        // Act
        var result = Record.Exception(() => _resourceCoordinator.Release(resources));

        // Assert
        Assert.Null(result);
        Assert.All(resources, resource => Assert.Equal(ResourceState.Idle, resource.State));
    }

    [Fact]
    public void Acquire_WhenResourceIsNotRegistered_ThrowsKeyNotFoundException()
    {
        // Arrange
        string[] resourceIds = ["R_UNKNOWN"];

        // Act
        var result = Record.Exception(
            () => _resourceCoordinator.Acquire(resourceIds, CancellationToken.None));

        // Assert
        Assert.IsType<KeyNotFoundException>(result);
    }
}
