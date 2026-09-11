using ManufacturingSystem.Modules.ResourceManagement;

namespace ManufacturingSystem.Test.Modules.ResourceManagement;

public sealed class ResourceTests
{
    private const string ResourceId = "R_A";
    private readonly Resource _resource;

    public ResourceTests()
    {
        _resource = new Resource(ResourceId);
    }

    [Fact]
    public void Acquire_WhenIdle_MarksResourceBusy()
    {
        // Arrange
        _resource.Release(CancellationToken.None);

        // Act
        var result = _resource.Acquire(CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(ResourceState.Busy, _resource.State);
    }

    [Fact]
    public void Acquire_WhenBusy_ReturnsFalse()
    {
        // Arrange
        _resource.Release(CancellationToken.None);
        _resource.Acquire(CancellationToken.None);

        // Act
        var result = _resource.Acquire(CancellationToken.None);

        // Assert
        Assert.False(result);
        Assert.Equal(ResourceState.Busy, _resource.State);
    }

    [Fact]
    public void Release_WhenBusy_MarksResourceIdle()
    {
        // Arrange
        _resource.Release(CancellationToken.None);
        _resource.Acquire(CancellationToken.None);

        // Act
        var result = _resource.Release(CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(ResourceState.Idle, _resource.State);
    }

    [Fact]
    public void Acquire_WhenCancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        // Act
        var result = Record.Exception(() => _resource.Acquire(cancellation.Token));

        // Assert
        Assert.IsType<OperationCanceledException>(result);
    }
}
