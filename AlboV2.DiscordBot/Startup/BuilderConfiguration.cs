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
using Serilog.Sinks.Grafana.Loki;
using Serilog;
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
        
        var enableRemoteLogging = configuration["EnableRemoteLogging"];

        if (string.IsNullOrEmpty(enableRemoteLogging) || !bool.TryParse(enableRemoteLogging, out _))
        {
            throw new ArgumentException("EnableRemoteLogging option not specified or invalid parameter provided - must be provided and set to true or false");
        }

        if (enableRemoteLogging.ToLower() == "true")
        {
            var lokiUsername = configuration["LokiUsername"];
            var lokiApiToken = configuration["LokiApiToken"];
            var lokiUrl = configuration["LokiUrl"];

            if (string.IsNullOrEmpty(lokiUrl) || string.IsNullOrEmpty(lokiUsername) || string.IsNullOrEmpty(lokiApiToken))
            {
                throw new ArgumentException("Endpoint, Username and ApiToken must be provided for Loki Instance");
            }
        }
    }

    public static void ConfigureRemoteLogging(WebApplicationBuilder builder)
    {

        var environmentName = builder.Environment.EnvironmentName;

        builder.Host.UseSerilog((context, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console();

            var enableRemoteLogging = context.Configuration.GetValue<bool>("EnableRemoteLogging");

            if (!enableRemoteLogging) return;
            // Even though these parameters would have been validated already, double check just in case
            var lokiUrl = builder.Configuration["LokiUrl"];
            var lokiUsername = builder.Configuration["LokiUsername"];
            var lokiApiToken = builder.Configuration["LokiApiToken"];

            if (string.IsNullOrEmpty(lokiUrl) || string.IsNullOrEmpty(lokiUsername) ||
                string.IsNullOrEmpty(lokiApiToken))
            {
                throw new ArgumentException(
                    "LokiUrl,LokiUsername and lokiApiToken must be provided if EnableRemoteLogging set to true");
            }

            configuration
                .WriteTo.GrafanaLoki(
                    lokiUrl,
                    labels: new List<LokiLabel>
                    {
                        new() { Key = "app", Value = "Albo" },
                        new() { Key = "environment", Value = environmentName },
                    },
                    credentials: new LokiCredentials
                    {
                        Login = lokiUsername,
                        Password = lokiApiToken
                    });
        });

    }
}