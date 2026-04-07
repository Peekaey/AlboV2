using Microsoft.Extensions.Logging;
using NetCord.Gateway;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace AlboV2.Shared.Helpers;

public static partial class LoggingExtensions
{
    public static IDisposable? BeginInteractionScope(this ILogger logger, ApplicationCommandContext context)
    {
        return logger.BeginScope(new Dictionary<string, object>
        {
            ["CommandName"] = context.Interaction.Data.Name,
            ["UserId"] = context.User.Id,
            ["GuildId"] = context.Interaction.GuildId ?? 0
        });
    }
    
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Started processing interaction.")]
    public static partial void LogInteractionStart(this ILogger logger);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "Completed interaction successfully in {DurationS}s.")]
    public static partial void LogInteractionSuccess(this ILogger logger, int durationS);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Error,
        Message = "Interaction failed after {DurationS}s.")]
    public static partial void LogInteractionError(this ILogger logger, Exception ex, int durationS);
    
    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Information,
        Message ="Started processing ScheduledTask.")]
    public static partial void LogScheduledTaskStart(this ILogger logger);
    
    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Information,
        Message = "Completed processing ScheduledTask in {DurationS}s.")]
    public static partial void LogScheduledTaskSuccess(this ILogger logger, int durationS);
    
    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Information,
        Message = "ScheduledTask failed after {DurationS}s.")]
    public static partial void LogScheduledTaskError(this ILogger logger, Exception ex, int durationS);
    
    
}