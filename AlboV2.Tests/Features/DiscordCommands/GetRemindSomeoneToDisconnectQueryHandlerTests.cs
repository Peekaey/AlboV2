using AlboV2.Features.DiscordCommands;

namespace AlboV2.Tests.Features.DiscordCommands;

public class GetRemindSomeoneToDisconnectQueryHandlerTests : IDisposable
{
    private readonly string _targetDirectory;

    public GetRemindSomeoneToDisconnectQueryHandlerTests()
    {
        // Set up the directory path
        _targetDirectory = Path.Combine(AppContext.BaseDirectory, "Assets", "Albo");
        
        // Ensure a clean state before the test starts
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

        var handler = new GetRemindSomeoneToDisconnectQueryHandler();
        var query = new GetRemindSomeoneToDisconnectQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.fileResponse);
        
        Assert.StartsWith("albo", result.fileResponse.fileName);
        Assert.EndsWith(".mov", result.fileResponse.fileName);
        
        Assert.True(result.fileResponse.content.Length > 0);
        
        result.fileResponse.content.Dispose();
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