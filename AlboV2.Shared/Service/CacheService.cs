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
                var cachedData = await _hybridCache.GetOrCreateAsync(
                    "holidays",
                    async cancellationToken =>
                    {
                        _logger.LogInformation("Cache miss - fetching latest holidays from Nager API");
                        using var holidayClient = new HolidayClient();
                        return await holidayClient.GetHolidaysAsync(localDate.Year, "AU", cancellationToken);
                    },
                    new HybridCacheEntryOptions
                    {
                        Expiration = TimeSpan.FromDays(28),
                        LocalCacheExpiration = TimeSpan.FromDays(28)
                    });

                if (cachedData is null)
                {
                    await _hybridCache.RemoveAsync("holidays");
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
            _logger.LogError(e, "Failed to public holidays");
            return null;
        }
    }
}