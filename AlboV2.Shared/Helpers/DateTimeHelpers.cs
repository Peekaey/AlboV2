using Nager.Date;

namespace AlboV2.Shared.Helpers;

public static class DateTimeHelpers
{
    public static bool IsPublicHoliday(DateTime utcDate, string ianaTimeZoneId)
    {
        var localDate = ConvertUtcToLocalTime(utcDate, ianaTimeZoneId);
        var auHolidays = HolidaySystem.GetHolidays(localDate.Year, CountryCode.AU);
        var holidayToday = auHolidays.FirstOrDefault(x => x.Date == localDate.Date);
        
        if (holidayToday == null)
        {
            return false;
        }
        
        bool isNational = holidayToday.SubdivisionCodes == null || holidayToday.SubdivisionCodes.Length == 0;
        
        if (TimezoneToIsoCode.TryGetValue(ianaTimeZoneId, out var targetIsoCode))
        {
            // 2. Check if this specific state celebrates this holiday
            bool isStateSpecific = holidayToday.SubdivisionCodes?.Contains(targetIsoCode) ?? false;
            return isNational || isStateSpecific;
        }

        return isNational;
    }
    
    // Quartz.NET -> Negar.Date
    // IANA Timezone -> ISO Subdivision Code translator
    public static readonly Dictionary<string, string> TimezoneToIsoCode = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Australia/Sydney", "AU-NSW" },
        { "Australia/Melbourne", "AU-VIC" },
        { "Australia/Brisbane", "AU-QLD" },
        { "Australia/Perth", "AU-WA" },
        { "Australia/Adelaide", "AU-SA" },
        { "Australia/Hobart", "AU-TAS" },
        { "Australia/Darwin", "AU-NT" },
        { "Australia/Lord_Howe", "AU-NSW" },
        { "Australia/Broken_Hill", "AU-NSW" },
        { "Australia/Eucla", "AU-WA" }
    };

    public static DateTime ConvertUtcToLocalTime(DateTime utc, string ianaTimeZoneId)
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(ianaTimeZoneId);
        return TimeZoneInfo.ConvertTimeFromUtc(utc, tz);
    }
}