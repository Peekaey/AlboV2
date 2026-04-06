using System.Diagnostics;
using AlboV2.Features.DiscordCommands;
using AlboV2.Shared.Helpers;
using Mediator;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace AlboV2.DiscordBot.DiscordCommands;

public class RemindSomeoneToDisconnectCommand : ApplicationCommandModule<ApplicationCommandContext>
{
    private readonly ILogger<RemindSomeoneToDisconnectCommand> _logger;
    private readonly IMediator _mediator;

    public RemindSomeoneToDisconnectCommand(ILogger<RemindSomeoneToDisconnectCommand> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }
    
    [SlashCommand("remind_someone_to_disconnect", "reminds someone about the right to disconnect")]
    public async Task SendRemindSomeoneToDisconnectCommand([SlashCommandParameter(Description = "User to remind")] User wagie)
    {
        using var scope = _logger.BeginInteractionScope(Context);
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInteractionStart();

        try
        {
            await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());
            RemindSomeoneToDisconnectResult result = await _mediator.Send(new GetRemindSomeoneToDisconnectQuery());
            
            AttachmentProperties attachment =
                new AttachmentProperties(result.fileResponse.fileName, result.fileResponse.content);

            await Context.Interaction.SendFollowupMessageAsync(
                new InteractionMessageProperties
                {
                    Attachments = new List<AttachmentProperties> { attachment },
                    Content = $"<@{wagie.Id}> — The right to disconnect is now law. Because if you're not being paid 24 hours a day, you shouldn't be on call 24 hours a day"
                });

            _logger.LogInteractionSuccess(stopwatch.Elapsed.TotalSeconds);
            
        }
        catch (Exception e)
        {
            _logger.LogInteractionError(e, stopwatch.Elapsed.TotalSeconds);

            await Context.Interaction.SendFollowupMessageAsync(new InteractionMessageProperties
            {
                Content = "Unexpected error occurred when running the remind_someone_to_disconnect command"
            });
        }
    }
    
}