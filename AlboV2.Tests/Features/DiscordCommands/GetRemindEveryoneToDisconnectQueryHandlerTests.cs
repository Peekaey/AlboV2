using AlboV2.Features.DiscordCommands;

namespace AlboV2.Tests.Features.DiscordCommands;

[Collection("Albo Tests")]
public class GetRemindEveryoneToDisconnectQueryHandlerTests : IDisposable
{
    private readonly string _targetDirectory;

    public GetRemindEveryoneToDisconnectQueryHandlerTests()
    {
        _targetDirectory = Path.Combine(AppContext.BaseDirectory, "Assets", "Albo");
        CleanupTestFiles();
    }

    [Fact]
    public async Task Handle_WhenFileExists_ReturnsRemindEveryoneToDisconnectResult()
    {
        // Arrange
        Directory.CreateDirectory(_targetDirectory);

        for (int i = 1; i <= 6; i++)
        {
            string filePath = Path.Combine(_targetDirectory, $"albo{i}.mov");
            await File.WriteAllBytesAsync(filePath, new byte[] { 0x00, 0x00 });
        }

        var handler = new GetRemindEveryoneToDisconnectQueryHandler();
        var query = new GetRemindEveryoneToDisconnectQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.fileResponse);

        Assert.StartsWith("albo", result.fileResponse.fileName);
        Assert.EndsWith(".mov", result.fileResponse.fileName);
        Assert.Equal("video/quicktime", result.fileResponse.contentType);

        Assert.True(result.fileResponse.content.Length > 0);

        await result.fileResponse.content.DisposeAsync();
    }

    [Fact]
    public async Task Handle_WhenFileDoesNotExist_ThrowsFileNotFoundException()
    {
        // Arrange
        var handler = new GetRemindEveryoneToDisconnectQueryHandler();
        var query = new GetRemindEveryoneToDisconnectQuery();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FileNotFoundException>(async () =>
            await handler.Handle(query, CancellationToken.None));

        Assert.StartsWith("albo", exception.Message);
        Assert.EndsWith(".mov", exception.Message);
        Assert.StartsWith(_targetDirectory, exception.FileName);
    }

    public void Dispose()
    {
        CleanupTestFiles();
    }

    private void CleanupTestFiles()
    {
        if (Directory.Exists(_targetDirectory))
        {
            Directory.Delete(_targetDirectory, true);
        }
    }
}