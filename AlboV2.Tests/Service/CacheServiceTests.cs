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


    private static readonly PublicHoliday[] SampleHolidays =
    [
        new() { Date = new DateTime(2026, 1, 1), Counties = null },  
        new() { Date = new DateTime(2026, 4, 25), Counties = null },
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
    public async Task GetCachedHolidays_CachingEnabled_Uses28DayRollingWindow_WhenFarFromYearEnd()
    {
        // Arrange — Jan 1 is ~365 days from year-end, well above the 28-day threshold
        _fakeHybridCache.ReturnValue = SampleHolidays;

        // Act
        await CreateSut().GetCachedHolidays(new DateTime(2026, 1, 1), "Australia/Sydney");

        // Assert — rolling 28-day window applies when > 28 days remain in the year
        Assert.NotNull(_fakeHybridCache.CapturedOptions);
        Assert.Equal(TimeSpan.FromDays(28), _fakeHybridCache.CapturedOptions.Expiration);
        Assert.Equal(TimeSpan.FromDays(28), _fakeHybridCache.CapturedOptions.LocalCacheExpiration);
    }

    [Fact]
    public async Task GetCachedHolidays_CachingEnabled_AlignsToYearEnd_WhenWithin28Days()
    {
        // Arrange — Dec 10 is 21 days 23:59:59 from year-end, within the 28-day threshold
        var localDate = new DateTime(2026, 12, 10);
        var expectedTtl = new DateTime(2026, 12, 31, 23, 59, 59) - localDate; // 21d 23:59:59
        _fakeHybridCache.ReturnValue = SampleHolidays;

        // Act
        await CreateSut().GetCachedHolidays(localDate, "Australia/Sydney");

        // Assert — TTL aligns to year-end, not a full 28 days
        Assert.NotNull(_fakeHybridCache.CapturedOptions);
        Assert.Equal(expectedTtl, _fakeHybridCache.CapturedOptions.Expiration);
        Assert.Equal(expectedTtl, _fakeHybridCache.CapturedOptions.LocalCacheExpiration);
    }

    [Fact]
    public async Task GetCachedHolidays_CachingEnabled_EnforcesMinimumOneHourTtl_NearMidnightOnDecember31()
    {
        // Arrange — 23:58 on Dec 31 leaves only ~2 minutes to year-end without the guard
        _fakeHybridCache.ReturnValue = SampleHolidays;

        // Act
        await CreateSut().GetCachedHolidays(new DateTime(2026, 12, 31, 23, 58, 0), "Australia/Sydney");

        // Assert — minimum 1-hour TTL prevents a near-zero expiration on the last night of the year
        Assert.NotNull(_fakeHybridCache.CapturedOptions);
        Assert.Equal(TimeSpan.FromHours(1), _fakeHybridCache.CapturedOptions.Expiration);
        Assert.Equal(TimeSpan.FromHours(1), _fakeHybridCache.CapturedOptions.LocalCacheExpiration);
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
        Assert.Equal("holidays_AU_2026", _fakeHybridCache.LastRemovedKey);
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

    #region GetCachedHolidays - Cache Key Isolation

    [Theory]
    [InlineData(2025, "holidays_AU_2025")]
    [InlineData(2026, "holidays_AU_2026")]
    [InlineData(2027, "holidays_AU_2027")]
    public async Task GetCachedHolidays_UsesCacheKeyWithYear(int year, string expectedKey)
    {
        // Arrange
        _fakeHybridCache.ReturnValue = SampleHolidays;

        // Act
        await CreateSut().GetCachedHolidays(new DateTime(year, 1, 1), "Australia/Sydney");

        // Assert
        Assert.Equal(expectedKey, _fakeHybridCache.LastGetOrCreateKey);
    }

    [Fact]
    public async Task GetCachedHolidays_DifferentYears_UseDifferentCacheKeys()
    {
        // Arrange — a second fake so both calls are independent
        var fake2026 = new FakeHybridCache { ReturnValue = SampleHolidays };
        var fake2027 = new FakeHybridCache { ReturnValue = SampleHolidays };
        var config = BuildConfiguration(enableCaching: true);
        var sut2026 = new CacheService(fake2026, _mockLogger.Object, config);
        var sut2027 = new CacheService(fake2027, _mockLogger.Object, config);

        // Act
        await sut2026.GetCachedHolidays(new DateTime(2026, 1, 1), "Australia/Sydney");
        await sut2027.GetCachedHolidays(new DateTime(2027, 1, 1), "Australia/Sydney");

        // Assert — each year maps to a distinct key
        Assert.NotEqual(fake2026.LastGetOrCreateKey, fake2027.LastGetOrCreateKey);
        Assert.Equal("holidays_AU_2026", fake2026.LastGetOrCreateKey);
        Assert.Equal("holidays_AU_2027", fake2027.LastGetOrCreateKey);
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
        public string? LastGetOrCreateKey { get; private set; }
        public HybridCacheEntryOptions? CapturedOptions { get; private set; }

        public override ValueTask<T> GetOrCreateAsync<TState, T>(
            string key, TState state, Func<TState, CancellationToken, ValueTask<T>> factory,
            HybridCacheEntryOptions? options = null, IEnumerable<string>? tags = null,
            CancellationToken cancellationToken = default)
        {
            GetOrCreateCallCount++;
            LastGetOrCreateKey = key;
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
