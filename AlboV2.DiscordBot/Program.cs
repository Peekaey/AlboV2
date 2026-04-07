using AlboV2.DiscordBot.Startup;

namespace AlboV2.DiscordBot;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        BuilderConfiguration.ConfigureMiscServicesBuilder(builder);
        BuilderConfiguration.ConfigureServicesBuilder(builder);
        BuilderConfiguration.ValidateEnvironmentVariables(builder);

        BuilderConfiguration.ConfigureNetCordBuilder(builder);
        BuilderConfiguration.ConfigureRemoteLogging(builder);
        ScheduleTaskConfiguration.ConfigureQuartzWithBackgroundJob(builder);
        var app = builder.Build();
        
        AppConfiguration.ConfigureApp(app);
        
        app.Run();
    }
}