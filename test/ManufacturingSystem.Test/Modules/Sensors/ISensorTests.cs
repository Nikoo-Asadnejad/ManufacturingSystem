using System.ComponentModel.DataAnnotations;
using Broadcaster;
using ManufacturingSystem.Sensors;
using Moq;

namespace ManufacturingSystem.Test.Modules.Sensors;

public sealed class ISensorTests
{
    [Fact]
    public async Task ReadAsync_Broadcasts()
    {
        // Arrange
        var broadcaster = MockBroadcaster();

        ISensor sensor = new TemperatureSensor(broadcaster.Object);

        // Act
        await sensor.ReadAsync(CancellationToken.None);

        // Assert
        Assert.Equal(SensorType.Temperature, sensor.SensorType);
        broadcaster.Verify(b=>
            b.BroadcastAsync(It.IsAny<IBroadcastEvent>() , 
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static Mock<IBroadcaster> MockBroadcaster()
    {
        var broadcaster = new Mock<IBroadcaster>();
        broadcaster
            .Setup(configuredBroadcaster => configuredBroadcaster.BroadcastAsync(
                It.IsAny<IBroadcastEvent>(),
                CancellationToken.None));
        return broadcaster;
    }
}
