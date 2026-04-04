using AlboV2.Services.DiscordCommands;

namespace AlboV2.Tests.MediatR;

public class GetPingQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnPongMessage()
    {
        // Arrange
        var handler = new GetPingQueryHandler();
        var query = new GetPingQuery();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Pong!", result.Message);
    }
}