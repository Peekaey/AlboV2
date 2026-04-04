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
        Message = "Completed interaction successfully in {DurationMs}ms.")]
    public static partial void LogInteractionSuccess(this ILogger logger, double durationMs);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Error,
        Message = "Interaction failed after {DurationMs}ms.")]
    public static partial void LogInteractionError(this ILogger logger, Exception ex, double durationMs);
}