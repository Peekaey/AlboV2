using AlboV2.DiscordBot.DiscordCommands.Internal;
using AlboV2.Features.DiscordCommands;
using AlboV2.Features.ScheduledTasks;
using AlboV2.Shared.Helpers;
using AlboV2.Shared.Service;
using Mediator;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Rest;

namespace AlboV2.DiscordBot.Startup;

public static class BuilderConfiguration
{
    public static void ConfigureNetCordBuilder(WebApplicationBuilder  builder)
    {
        Console.WriteLine("Executing ConfigureNetcordBuilder...");
        IEntityToken restClientToken = new BotToken(builder.Configuration["DISCORD_BOT_TOKEN"] ?? throw new ArgumentException("DISCORD_BOT_TOKEN must be provided"));
        builder.Services.AddDiscordGateway(options =>
            {
                options.Intents = GatewayIntents.GuildMessages
                                  | GatewayIntents.GuildUsers
                                  | GatewayIntents.DirectMessages
                                  | GatewayIntents.MessageContent
                                  | GatewayIntents.Guilds
                                  | GatewayIntents.GuildPresences
                                  | GatewayIntents.GuildUsers;
                options.Token = builder.Configuration["DISCORD_BOT_TOKEN"];
            })
            .AddApplicationCommands()
            .AddGatewayHandlers(typeof(Program).Assembly)
            .AddSingleton<RestClient>(sp => new RestClient(restClientToken));
    }

    public static void ConfigureMiscServicesBuilder(WebApplicationBuilder builder)
    {
        Console.WriteLine("Executing ConfigureMiscServicesBuilder...");
        
        builder.Services.AddLogging(logger =>
        {
            logger.ClearProviders();
            logger.AddConsole();
            logger.AddDebug();
        });
        
        builder.Services.AddMediator((MediatorOptions options) =>
        {
            options.Namespace = "AlboV2.Mediator";
            options.ServiceLifetime = ServiceLifetime.Singleton;
            // Only available from v3:
            options.GenerateTypesAsInternal = true;
            options.NotificationPublisherType = typeof(Mediator.ForeachAwaitPublisher);
            // options.Assemblies = [typeof(...)];
            // options.Types = [typeof(IModuleMarker)];
            options.PipelineBehaviors = [];
            options.StreamPipelineBehaviors = [];
            // Only available from v3.1:
            // options.CachingMode = CachingMode.Eager;
        });

        builder.Services.AddHybridCache();
        
        builder.Services.AddControllers();
        
    }

    public static void ConfigureServicesBuilder(WebApplicationBuilder builder)
    {
        Console.WriteLine("Executing ConfigureServicesBuilder...");
        
        builder.Services.AddTransient<ISendRemindEveryoneToDisconnectScheduledMessage, RemindEveryoneToDisconnectScheduledMessage>();
        builder.Services.AddSingleton<IDateTimeHelperService, DateTimeHelperService>();
        builder.Services.AddSingleton<ICacheService, CacheService>();
    }

    public static void ValidateEnvironmentVariables(WebApplicationBuilder builder)
    {
        Console.WriteLine("Executing ValidateEnvironmentVariables...");
        
        var configuration = builder.Configuration;
        
        var discordBotToken = configuration["DISCORD_BOT_TOKEN"];
        if (string.IsNullOrEmpty(discordBotToken))
        {
            throw new ArgumentNullException(discordBotToken, "DISCORD_BOT_TOKEN must be provided");
        }
        
        var discordReminderChannelId = configuration["DISCORD_REMINDER_CHANNELID"];
        if (string.IsNullOrEmpty(discordReminderChannelId))
        {
            throw new ArgumentNullException(discordReminderChannelId, "DISCORD_REMINDER_CHANNELID must be provided");
        }
        
        var ianaTimezoneId = configuration["TimezoneId"];
        if (string.IsNullOrEmpty(ianaTimezoneId))
        {
            throw new ArgumentNullException(ianaTimezoneId, "TimezoneId must be provided");
        }
        
        if (!DateTimeHelperService.TimezoneToIsoCode.ContainsKey(ianaTimezoneId))
        {
            throw new ArgumentNullException(ianaTimezoneId, "iana TimezoneId specific to Australia must be provided");
        }
        Console.WriteLine("Provided TimezoneId: " + ianaTimezoneId);
        
        var enableCaching = configuration["EnableCaching"];
        if (string.IsNullOrEmpty(enableCaching) || !bool.TryParse(enableCaching, out _))
        {
            throw new ArgumentException("EnableCaching option not specified or invalid parameter provided - must be provided and set to true or false");
        }
    }
    


}