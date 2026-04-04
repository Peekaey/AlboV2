using AlboV2.DiscordBot.Startup;

namespace AlboV2.DiscordBot;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        BuilderConfiguration.ValidateEnvironmentVariables(builder);
        BuilderConfiguration.ConfigureMiscServicesBuilder(builder);
        BuilderConfiguration.ConfigureServicesBuilder(builder);
        BuilderConfiguration.ConfigureNetCordBuilder(builder);
        
        var app = builder.Build();
        
        AppConfiguration.ConfigureApp(app);
        
        app.Run();
    }
}