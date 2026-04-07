using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nager.Holiday;

namespace AlboV2.Shared.Service;

public class CacheService : ICacheService
{
    private readonly HybridCache _hybridCache;
    private readonly ILogger<CacheService> _logger;
    private readonly IConfiguration _configuration;

    public CacheService(HybridCache hybridCache, ILogger<CacheService> logger, IConfiguration configuration)
    {
        _hybridCache = hybridCache;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<PublicHoliday[]?> GetCachedHolidays(DateTime localDate, string ianaTimeZoneId)
    {
        try
        {
            var enableCaching = _configuration.GetValue<bool>("EnableCaching");
            
            if (enableCaching == true)
            {
                string cacheKey = $"holidays_AU_{localDate.Year}";

                var endOfYear = new DateTime(localDate.Year, 12, 31, 23, 59, 59);
                var timeUntilEndOfYear = endOfYear - localDate;

                // Use a year-end-aligned TTL in the last 28 days of the year to prevent
                // the cache entry bleeding into the new year. For the rest of the year,
                // use a rolling 28-day window. A minimum of 1 hour guards against a
                // near-zero TTL on December 31 near midnight.
                var expirationTime = timeUntilEndOfYear > TimeSpan.FromDays(28)
                    ? TimeSpan.FromDays(28)
                    : TimeSpan.FromTicks(Math.Max(timeUntilEndOfYear.Ticks, TimeSpan.FromHours(1).Ticks));
                
                var cachedData = await _hybridCache.GetOrCreateAsync(
                    cacheKey,
                    async cancellationToken =>
                    {
                        _logger.LogInformation("Cache miss - fetching latest holidays from Nager API");
                        using var holidayClient = new HolidayClient();
                        return await holidayClient.GetHolidaysAsync(localDate.Year, "AU", cancellationToken);
                    },
                    new HybridCacheEntryOptions
                    {
                        Expiration = expirationTime,
                        LocalCacheExpiration = expirationTime
                    });

                if (cachedData is null)
                {
                    await _hybridCache.RemoveAsync(cacheKey);
                }

                return cachedData;
            }
            else
            {
                _logger.LogInformation("Fetching latest holidays from Nager API");
                using var holidayClient = new HolidayClient();
                return await holidayClient.GetHolidaysAsync(localDate.Year, "AU");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to fetch public holidays for year {Year} and time zone {TimeZoneId}", localDate.Year, ianaTimeZoneId);
            return null;
        }
    }
}