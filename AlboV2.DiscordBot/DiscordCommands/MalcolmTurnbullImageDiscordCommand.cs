using System.Diagnostics;
using AlboV2.Features.DiscordCommands;
using AlboV2.Shared.Helpers;
using Mediator;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace AlboV2.DiscordBot.DiscordCommands;

public class MalcolmTurnbullImageDiscordCommand :ApplicationCommandModule<ApplicationCommandContext>
{
    private readonly ILogger<MalcolmTurnbullImageDiscordCommand> _logger;
    private readonly IMediator _mediator;

    public MalcolmTurnbullImageDiscordCommand(ILogger<MalcolmTurnbullImageDiscordCommand> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [SlashCommand("malcolmturnbull", "sends the same photo of malcolm turnbull")]
    public async Task SendMalcolmTurnbullPhoto()
    {
        using var scope = _logger.BeginInteractionScope(Context);
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInteractionStart();

        try
        {
            await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());
            MalcolmTurnbullImageResult result = await _mediator.Send(new GetMalcolmTurnbullImageQuery());
            
            AttachmentProperties attachment = new AttachmentProperties(result.fileResponse.fileName, result.fileResponse.content);

            await Context.Interaction.SendFollowupMessageAsync(
                new InteractionMessageProperties
                {
                    Attachments = new List<AttachmentProperties> { attachment },
                });

            _logger.LogInteractionSuccess(stopwatch.Elapsed.TotalSeconds);

        }
        catch (Exception e)
        {
            _logger.LogInteractionError(e, stopwatch.Elapsed.TotalSeconds);

            await Context.Interaction.SendFollowupMessageAsync(new InteractionMessageProperties
            {
                Content = "Unexpected error occured when running the malcolmturnbull command"
            });
        }
        
    }
}