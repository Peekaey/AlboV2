using Microsoft.Extensions.Logging;
using Nager.Holiday;

namespace AlboV2.Shared.Service;

public class DateTimeHelperService : IDateTimeHelperService
{
    private readonly ILogger<DateTimeHelperService> _logger;
    
    public DateTimeHelperService(ILogger<DateTimeHelperService> logger)
    {
        _logger = logger;
    }
    
    // Using Nager.Date package
    // [Obsolete]
    // public static bool IsPublicHoliday(DateTime utcDate, string ianaTimeZoneId)
    // {
    //     var localDate = ConvertUtcToLocalTime(utcDate, ianaTimeZoneId);
    //     var auHolidays = HolidaySystem.GetHolidays(localDate.Year, CountryCode.AU);
    //     var holidayToday = auHolidays.FirstOrDefault(x => x.Date == localDate.Date);
    //     
    //     if (holidayToday == null)
    //     {
    //         return false;
    //     }
    //     
    //     bool isNational = holidayToday.SubdivisionCodes == null || holidayToday.SubdivisionCodes.Length == 0;
    //     
    //     if (TimezoneToIsoCode.TryGetValue(ianaTimeZoneId, out var targetIsoCode))
    //     {
    //         // 2. Check if this specific state celebrates this holiday
    //         bool isStateSpecific = holidayToday.SubdivisionCodes?.Contains(targetIsoCode) ?? false;
    //         return isNational || isStateSpecific;
    //     }
    //
    //     return isNational;
    // }

    public async Task<bool> IsDatePublicHoliday(DateTime utcDate, string ianaTimeZoneId)
    {
        var localDate = ConvertUtcToLocalTime(utcDate, ianaTimeZoneId);
        var allYearAuHolidays = await GetHolidaysAsync(localDate, ianaTimeZoneId);

        // Means no Australian public holidays 0_o
        if (allYearAuHolidays is null || allYearAuHolidays.Length == 0)
        {
            return false;
        }

        // Reduce collection size
        var holidaysOnCurrentDate = allYearAuHolidays.Where(x => x.Date == localDate.Date).ToList();
        
        if (!holidaysOnCurrentDate.Any())
        {
            return false;
        }
        
        // Country wide public holiday
        if (holidaysOnCurrentDate.Any(x => x.Counties == null || x.Counties.Length == 0))
        {
            return true;
        }
        
        //  State-Specific Holidays
        if (TimezoneToIsoCode.TryGetValue(ianaTimeZoneId, out var targetIsoCode))
        {
            return holidaysOnCurrentDate.Any(x => x.Counties != null && x.Counties.Contains(targetIsoCode));
        }
        
        return false;
        
    }

    private async Task<PublicHoliday[]?> GetHolidaysAsync(DateTime localDate, string ianaTimeZoneId)
    {
        using var holidayClient = new HolidayClient();
        var holidays = await holidayClient.GetHolidaysAsync(localDate.Year, "AU");
        return holidays;
    }
    
    // Quartz.NET -> Negar.Date
    // IANA Timezone -> ISO Subdivision Code translator
    public readonly Dictionary<string, string> TimezoneToIsoCode = new(StringComparer.OrdinalIgnoreCase)
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

    public bool IsValidTimezone(string ianaTimezoneId) => TimezoneToIsoCode.ContainsKey(ianaTimezoneId);
    
    private  DateTime ConvertUtcToLocalTime(DateTime utc, string ianaTimeZoneId)
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(ianaTimeZoneId);
        return TimeZoneInfo.ConvertTimeFromUtc(utc, tz);
    }
}