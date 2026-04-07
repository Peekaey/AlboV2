using AlboV2.Features.DiscordCommands;
using AlboV2.Features.ScheduledTasks;
using NetCord.Rest;

namespace AlboV2.DiscordBot.DiscordCommands.Internal;

public class RemindEveryoneToDisconnectScheduledMessage : ISendRemindEveryoneToDisconnectScheduledMessage
{
    private readonly ILogger<RemindEveryoneToDisconnectScheduledMessage> _logger;
    private readonly RestClient _restClient;
    private readonly IConfiguration _configuration;
    
    public RemindEveryoneToDisconnectScheduledMessage(ILogger<RemindEveryoneToDisconnectScheduledMessage> logger, RestClient restClient,
        IConfiguration configuration)
    {
        _logger = logger;
        _restClient = restClient;
        _configuration = configuration;
    }

    public async Task SendRemindEveryoneToDisconnectScheduledMessage(RemindEveryoneToDisconnectResult result)
    {
        AttachmentProperties attachment = new AttachmentProperties(result.fileResponse.fileName, result.fileResponse.content);
        
        var channelId = _configuration.GetValue<ulong>("DISCORD_REMINDER_CHANNELID");
        await _restClient.SendMessageAsync(channelId, new MessageProperties
        {
            Attachments = new List<AttachmentProperties> { attachment },
            Content = "@everyone — Just a reminder that the right to disconnect is now law. Because if you're not being paid 24 hours a day, you shouldn't be on call 24 hours a day"
        });
    }
    
}