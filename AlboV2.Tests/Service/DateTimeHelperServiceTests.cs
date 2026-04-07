using AlboV2.Shared.Service;
using Microsoft.Extensions.Logging;
using Moq;

namespace AlboV2.Tests.Service;

public class DateTimeHelperServiceTests
{
    private readonly Mock<ILogger<DateTimeHelperService>> _mockLogger;
    private readonly DateTimeHelperService _sut;

    public DateTimeHelperServiceTests()
    {
        _mockLogger = new Mock<ILogger<DateTimeHelperService>>();
        _sut = new DateTimeHelperService(_mockLogger.Object);
    }

    #region IsValidTimezone

    [Theory]
    [InlineData("Australia/Sydney")]
    [InlineData("Australia/Melbourne")]
    [InlineData("Australia/Brisbane")]
    [InlineData("Australia/Perth")]
    [InlineData("Australia/Adelaide")]
    [InlineData("Australia/Hobart")]
    [InlineData("Australia/Darwin")]
    [InlineData("Australia/Lord_Howe")]
    [InlineData("Australia/Broken_Hill")]
    [InlineData("Australia/Eucla")]
    public void IsValidTimezone_WithValidAustralianTimezone_ReturnsTrue(string timezone)
    {
        // Act
        var result = _sut.IsValidTimezone(timezone);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("America/New_York")]
    [InlineData("Europe/London")]
    [InlineData("Asia/Tokyo")]
    [InlineData("Invalid/Timezone")]
    [InlineData("")]
    public void IsValidTimezone_WithInvalidTimezone_ReturnsFalse(string timezone)
    {
        // Act
        var result = _sut.IsValidTimezone(timezone);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("australia/sydney")]
    [InlineData("AUSTRALIA/SYDNEY")]
    [InlineData("Australia/SYDNEY")]
    public void IsValidTimezone_IsCaseInsensitive_ReturnsTrue(string timezone)
    {
        // Act
        var result = _sut.IsValidTimezone(timezone);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region TimezoneToIsoCode

    [Theory]
    [InlineData("Australia/Sydney", "AU-NSW")]
    [InlineData("Australia/Melbourne", "AU-VIC")]
    [InlineData("Australia/Brisbane", "AU-QLD")]
    [InlineData("Australia/Perth", "AU-WA")]
    [InlineData("Australia/Adelaide", "AU-SA")]
    [InlineData("Australia/Hobart", "AU-TAS")]
    [InlineData("Australia/Darwin", "AU-NT")]
    [InlineData("Australia/Lord_Howe", "AU-NSW")]
    [InlineData("Australia/Broken_Hill", "AU-NSW")]
    [InlineData("Australia/Eucla", "AU-WA")]
    public void TimezoneToIsoCode_ContainsCorrectMappings(string timezone, string expectedIsoCode)
    {
        // Act
        var result = _sut.TimezoneToIsoCode[timezone];

        // Assert
        Assert.Equal(expectedIsoCode, result);
    }

    [Fact]
    public void TimezoneToIsoCode_ContainsTenEntries()
    {
        Assert.Equal(10, _sut.TimezoneToIsoCode.Count);
    }

    #endregion

    #region IsDatePublicHoliday
    // Integration tests — they call the Nager Holiday API over the network.
    // Mark with [Trait("Category", "Integration")] to allow filtering in CI.

    [Theory]
    [Trait("Category", "Integration")]
    [InlineData("2026-01-01", "Australia/Sydney", true)]   // New Year's Day  (national)
    [InlineData("2026-04-25", "Australia/Sydney", true)]   // ANZAC Day       (national)
    [InlineData("2026-12-25", "Australia/Sydney", true)]   // Christmas Day   (national)
    [InlineData("2026-12-28", "Australia/Sydney", true)]   // Boxing Day      (national, observed Mon 28 Dec because 26 Dec falls on Sat)
    [InlineData("2026-07-15", "Australia/Sydney", false)]  // Regular weekday (no holiday)
    public async Task IsDatePublicHoliday_WithNationalHolidays_ReturnsExpectedResult(
        string dateString, string timezone, bool expected)
    {
        // Arrange
        var localDate = DateTime.Parse(dateString);
        var tz = TimeZoneInfo.FindSystemTimeZoneById(timezone);
        var utcDate = TimeZoneInfo.ConvertTimeToUtc(localDate, tz);

        // Act
        var result = await _sut.IsDatePublicHoliday(utcDate, timezone);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [Trait("Category", "Integration")]
    // Melbourne Cup Day (first Tuesday of November) is a VIC-only public holiday
    [InlineData("2026-11-03", "Australia/Melbourne", true)]   // Melbourne Cup Day — VIC only
    [InlineData("2026-11-03", "Australia/Sydney", false)]     // Not a public holiday in NSW
    public async Task IsDatePublicHoliday_WithStateSpecificHoliday_ReturnsExpectedResult(
        string dateString, string timezone, bool expected)
    {
        // Arrange
        var localDate = DateTime.Parse(dateString);
        var tz = TimeZoneInfo.FindSystemTimeZoneById(timezone);
        var utcDate = TimeZoneInfo.ConvertTimeToUtc(localDate, tz);

        // Act
        var result = await _sut.IsDatePublicHoliday(utcDate, timezone);

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion
}

