using ManufacturingSystem.WorkflowEngine.ResourceManagement;

namespace ManufacturingSystem.Test.Modules.Workflow.ResourceManagement;

public sealed class ResourceTests
{
    private const string ResourceId = "R_A";
    private readonly Resource _resource;

    public ResourceTests()
    {
        _resource = new Resource(ResourceId);
    }

    [Fact]
    public async Task Acquire_WhenIdle_MarksResourceBusy()
    {
        // Act
        var result = await _resource.Acquire(CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(ResourceState.Busy, _resource.State);
    }

    [Fact]
    public async Task Acquire_WhenBusy_ReturnsFalse()
    {
        // Arrange
        await _resource.Acquire(CancellationToken.None);

        // Act
        var result = await _resource.Acquire(CancellationToken.None);

        // Assert
        Assert.False(result);
        Assert.Equal(ResourceState.Busy, _resource.State);
    }

    [Fact]
    public async Task Release_WhenBusy_MarksResourceIdle()
    {
        // Arrange
        await _resource.Acquire(CancellationToken.None);

        // Act
        var result = await _resource.Release(CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(ResourceState.Idle, _resource.State);
    }

    [Fact]
    public async Task Acquire_WhenCancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        // Act
        var result = await Record.ExceptionAsync(() => _resource.Acquire(cancellation.Token));

        // Assert
        Assert.IsAssignableFrom<OperationCanceledException>(result);
    }
}
