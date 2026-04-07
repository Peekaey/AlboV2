using Nager.Holiday;

namespace AlboV2.Shared.Service;

public interface IDateTimeHelperService
{
    Task<bool> IsDatePublicHoliday(DateTime utcDate, string ianaTimeZoneId);
    bool IsValidTimezone(string ianaTimezoneId);
    DateTime ConvertUtcToLocalTime(DateTime utc, string ianaTimeZoneId);
}