using System.Diagnostics;
using AlboV2.Features.DiscordCommands;
using AlboV2.Shared.Helpers;
using Mediator;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace AlboV2.DiscordBot.DiscordCommands;

public class RemindEveryoneToDisconnectCommand : ApplicationCommandModule<ApplicationCommandContext>
{
    private readonly ILogger<RemindEveryoneToDisconnectCommand> _logger;
    private readonly IMediator _mediator;

    public RemindEveryoneToDisconnectCommand(ILogger<RemindEveryoneToDisconnectCommand> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [SlashCommand("remind_everyone_to_disconnect", "reminds everyone in the channel about the right to disconnect")]
    public async Task SendRemindEveryoneToDisconnect()
    {
        using var scope = _logger.BeginInteractionScope(Context);
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInteractionStart();

        try
        {
            await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());
            RemindEveryoneToDisconnectResult result = await _mediator.Send(new GetRemindEveryoneToDisconnectQuery());

            AttachmentProperties attachment =
                new AttachmentProperties(result.fileResponse.fileName, result.fileResponse.content);
            
            await Context.Interaction.SendFollowupMessageAsync(
                new InteractionMessageProperties
                {
                    Attachments = new List<AttachmentProperties> { attachment },
                    Content = "@everyone — Just a reminder that the right to disconnect is now law. Because if you're not being paid 24 hours a day, you shouldn't be on call 24 hours a day"
                });

            _logger.LogInteractionSuccess(stopwatch.Elapsed.TotalSeconds);
            
        }
        catch (Exception e)
        {
            _logger.LogInteractionError(e, stopwatch.Elapsed.Seconds);

            await Context.Interaction.SendFollowupMessageAsync(new InteractionMessageProperties
            {
                Content = "Unexpected error occured when running the remind_everyone_to_disconnect command"
            });
        }
        
    }
}