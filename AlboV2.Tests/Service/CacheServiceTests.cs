using AlboV2.Shared.Service;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Nager.Holiday;

namespace AlboV2.Tests.Service;

public class CacheServiceTests
{
    private readonly FakeHybridCache _fakeHybridCache;
    private readonly Mock<ILogger<CacheService>> _mockLogger;

    // Represents a minimal set of AU public holidays for use across tests
    private static readonly PublicHoliday[] SampleHolidays =
    [
        new() { Date = new DateTime(2026, 1, 1), Counties = null },  // national
        new() { Date = new DateTime(2026, 4, 25), Counties = null }, // national
    ];

    public CacheServiceTests()
    {
        _fakeHybridCache = new FakeHybridCache();
        _mockLogger = new Mock<ILogger<CacheService>>();
    }

    private CacheService CreateSut(bool enableCaching = true) =>
        new(_fakeHybridCache, _mockLogger.Object, BuildConfiguration(enableCaching));

    private static IConfiguration BuildConfiguration(bool enableCaching) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["EnableCaching"] = enableCaching.ToString() })
            .Build();

    #region GetCachedHolidays - Caching Enabled

    [Fact]
    public async Task GetCachedHolidays_CachingEnabled_ReturnsCachedData()
    {
        // Arrange
        _fakeHybridCache.ReturnValue = SampleHolidays;

        // Act
        var result = await CreateSut().GetCachedHolidays(new DateTime(2026, 1, 1), "Australia/Sydney");

        // Assert
        Assert.Equal(SampleHolidays, result);
    }

    [Fact]
    public async Task GetCachedHolidays_CachingEnabled_CallsCacheExactlyOnce()
    {
        // Arrange
        _fakeHybridCache.ReturnValue = SampleHolidays;

        // Act
        await CreateSut().GetCachedHolidays(new DateTime(2026, 1, 1), "Australia/Sydney");

        // Assert
        Assert.Equal(1, _fakeHybridCache.GetOrCreateCallCount);
    }

    [Fact]
    public async Task GetCachedHolidays_CachingEnabled_DoesNotEvictCache_WhenDataReturned()
    {
        // Arrange
        _fakeHybridCache.ReturnValue = SampleHolidays;

        // Act
        await CreateSut().GetCachedHolidays(new DateTime(2026, 1, 1), "Australia/Sydney");

        // Assert
        Assert.Equal(0, _fakeHybridCache.RemoveAsyncCallCount);
    }

    [Fact]
    public async Task GetCachedHolidays_CachingEnabled_UsesCorrectExpiration()
    {
        // Arrange
        _fakeHybridCache.ReturnValue = SampleHolidays;

        // Act
        await CreateSut().GetCachedHolidays(new DateTime(2026, 1, 1), "Australia/Sydney");

        // Assert — 28-day TTL on both L1 and L2 cache layers
        Assert.NotNull(_fakeHybridCache.CapturedOptions);
        Assert.Equal(TimeSpan.FromDays(28), _fakeHybridCache.CapturedOptions.Expiration);
        Assert.Equal(TimeSpan.FromDays(28), _fakeHybridCache.CapturedOptions.LocalCacheExpiration);
    }

    #endregion

    #region GetCachedHolidays - Null Result (Cache Eviction)

    [Fact]
    public async Task GetCachedHolidays_CachingEnabled_ReturnsNull_WhenCacheReturnsNull()
    {
        // Arrange
        _fakeHybridCache.ReturnValue = null;

        // Act
        var result = await CreateSut().GetCachedHolidays(new DateTime(2026, 1, 1), "Australia/Sydney");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetCachedHolidays_CachingEnabled_EvictsCache_WhenCacheReturnsNull()
    {
        // Arrange
        _fakeHybridCache.ReturnValue = null;

        // Act
        await CreateSut().GetCachedHolidays(new DateTime(2026, 1, 1), "Australia/Sydney");

        // Assert — entry is removed so next call re-fetches from the API rather than serving null
        Assert.Equal(1, _fakeHybridCache.RemoveAsyncCallCount);
        Assert.Equal("holidays", _fakeHybridCache.LastRemovedKey);
    }

    #endregion

    #region GetCachedHolidays - Exception Handling

    [Fact]
    public async Task GetCachedHolidays_ReturnsNull_WhenExceptionIsThrown()
    {
        // Arrange
        _fakeHybridCache.ShouldThrow = true;

        // Act
        var result = await CreateSut().GetCachedHolidays(new DateTime(2026, 1, 1), "Australia/Sydney");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetCachedHolidays_DoesNotThrow_WhenExceptionIsThrown()
    {
        // Arrange
        _fakeHybridCache.ShouldThrow = true;

        // Act & Assert
        var exception = await Record.ExceptionAsync(() =>
            CreateSut().GetCachedHolidays(new DateTime(2026, 1, 1), "Australia/Sydney"));

        Assert.Null(exception);
    }

    #endregion

    #region GetCachedHolidays - Caching Disabled (Integration)

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetCachedHolidays_CachingDisabled_FetchesDirectlyFromApi()
    {
        // Act
        var result = await CreateSut(enableCaching: false)
            .GetCachedHolidays(new DateTime(2026, 1, 1), "Australia/Sydney");

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetCachedHolidays_CachingDisabled_NeverCallsHybridCache()
    {
        // Act
        await CreateSut(enableCaching: false)
            .GetCachedHolidays(new DateTime(2026, 1, 1), "Australia/Sydney");

        // Assert
        Assert.Equal(0, _fakeHybridCache.GetOrCreateCallCount);
    }

    #endregion
    
    private sealed class FakeHybridCache : HybridCache
    {
        public PublicHoliday[]? ReturnValue { get; set; }
        public bool ShouldThrow { get; set; }
        public int GetOrCreateCallCount { get; private set; }
        public int RemoveAsyncCallCount { get; private set; }
        public string? LastRemovedKey { get; private set; }
        public HybridCacheEntryOptions? CapturedOptions { get; private set; }

        public override ValueTask<T> GetOrCreateAsync<TState, T>(
            string key, TState state, Func<TState, CancellationToken, ValueTask<T>> factory,
            HybridCacheEntryOptions? options = null, IEnumerable<string>? tags = null,
            CancellationToken cancellationToken = default)
        {
            GetOrCreateCallCount++;
            CapturedOptions = options;

            if (ShouldThrow)
                throw new HttpRequestException("Nager API unavailable");

            var result = ReturnValue is null ? default : (T)(object)ReturnValue;
            return ValueTask.FromResult(result!);
        }

        public override ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            RemoveAsyncCallCount++;
            LastRemovedKey = key;
            return ValueTask.CompletedTask;
        }

        public override ValueTask SetAsync<T>(string key, T value,
            HybridCacheEntryOptions? options = null, IEnumerable<string>? tags = null,
            CancellationToken cancellationToken = default) => ValueTask.CompletedTask;

        public override ValueTask RemoveByTagAsync(IEnumerable<string> tags,
            CancellationToken cancellationToken = default) => ValueTask.CompletedTask;

        public override ValueTask RemoveByTagAsync(string tag,
            CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
    }
}
