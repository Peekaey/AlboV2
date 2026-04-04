using AlboV2.Features.DiscordCommands;
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
        
        builder.Services.AddControllers();
        
    }

    public static void ConfigureServicesBuilder(WebApplicationBuilder builder)
    {
        Console.WriteLine("Executing ConfigureServicesBuilder...");

        builder.Services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblyContaining<GetPingQuery>();
        });
    }

    public static void ValidateEnvironmentVariables(WebApplicationBuilder builder)
    {
        Console.WriteLine("Executing ValidateEnvironmentVariablesBuilder...");
        
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
    }
    

}