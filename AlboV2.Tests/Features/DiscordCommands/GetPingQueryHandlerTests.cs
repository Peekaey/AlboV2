using AlboV2.Features.DiscordCommands;

namespace AlboV2.Tests.Features.DiscordCommands;

public class GetPingQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnPongMessage()
    {
        // Arrange
        var handler = new GetPingQueryHandler();
        var query = new GetPingQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Pong!", result.Message);
    }
}