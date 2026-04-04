namespace AlboV2.DiscordBot.Startup;

public static class AppConfiguration
{
    public static void ConfigureApp(WebApplication app)
    {
        Console.WriteLine("Executing ConfigureApp...");
        
        app.UseAuthorization();
        
        app.MapControllers();
    }
}