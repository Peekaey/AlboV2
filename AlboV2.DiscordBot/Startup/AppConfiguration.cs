using NetCord.Hosting.Services;

namespace AlboV2.DiscordBot.Startup;

public static class AppConfiguration
{
    public static void ConfigureApp(WebApplication app)
    {
        Console.WriteLine("Executing ConfigureApp...");
        app.AddModules(typeof(Program).Assembly);
        app.UseAuthorization();
        
        app.MapControllers();
    }
}