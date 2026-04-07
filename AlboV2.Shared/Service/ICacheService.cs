using Nager.Holiday;

namespace AlboV2.Shared.Service;

public interface ICacheService
{
    Task<PublicHoliday[]?> GetCachedHolidays(DateTime localDate, string ianaTimeZoneId);
}