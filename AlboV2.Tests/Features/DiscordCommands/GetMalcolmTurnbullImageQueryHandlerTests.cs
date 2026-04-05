using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AlboV2.Features.DiscordCommands;
using Xunit;

namespace AlboV2.Tests.Features.DiscordCommands;

public class GetMalcolmTurnbullImageQueryHandlerTests : IDisposable
{
    private readonly string _targetDirectory;
    private readonly string _targetFilePath;

    public GetMalcolmTurnbullImageQueryHandlerTests()
    {
        _targetDirectory = Path.Combine(AppContext.BaseDirectory, "Assets", "Turnbull");
        _targetFilePath = Path.Combine(_targetDirectory, "MalcolmTurnbull.jpg");
        
        CleanupTestFile();
    }

    [Fact]
    public async Task Handle_WhenImageExists_ReturnsMalcolmTurnbullImageResult()
    {
        // Arrange
        Directory.CreateDirectory(_targetDirectory);
        
        // Create a dummy file to simulate the image
        byte[] dummyImageBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }; 
        await File.WriteAllBytesAsync(_targetFilePath, dummyImageBytes);
        
        var handler = new GetMalcolmTurnbullImageQueryHandler();
        var query = new GetMalcolmTurnbullImageQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.fileResponse);
        
        // Assuming FileResponse exposes these based on your constructor arguments
        Assert.Equal("MalcolmTurnbull.jpg", result.fileResponse.fileName); // Check file name
        Assert.Equal("image/jpeg", result.fileResponse.contentType);       // Check content type
        Assert.True(result.fileResponse.content.Length > 0);                // Ensure stream is open/populated

        // Clean up the opened stream so the file lock is released
        await result.fileResponse.content.DisposeAsync();
    }

    [Fact]
    public async Task Handle_WhenImageDoesNotExist_ThrowsFileNotFoundException()
    {
        // Arrange
        var handler = new GetMalcolmTurnbullImageQueryHandler();
        var query = new GetMalcolmTurnbullImageQuery();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FileNotFoundException>(async () => 
            await handler.Handle(query, CancellationToken.None));

        // The handler passes the file name as the "message" parameter in the exception
        Assert.Equal("MalcolmTurnbull.jpg", exception.Message);
        Assert.Equal(_targetFilePath, exception.FileName);
    }

    // Teardown method called after every test
    public void Dispose()
    {
        CleanupTestFile();
    }

    private void CleanupTestFile()
    {
        if (File.Exists(_targetFilePath))
        {
            File.Delete(_targetFilePath);
        }
    }
}