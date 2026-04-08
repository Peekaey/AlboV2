using System.Diagnostics;
using AlboV2.Features.DiscordCommands;
using AlboV2.Shared.Helpers;
using Mediator;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace AlboV2.DiscordBot.DiscordCommands;

public class DisconnectCheckCommand : ApplicationCommandModule<ApplicationCommandContext>
{
    private readonly ILogger<DisconnectCheckCommand> _logger;
    private readonly IMediator _mediator;
    
    public DisconnectCheckCommand(ILogger<DisconnectCheckCommand> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [SlashCommand("disconnect_check", "check if someone has disconnected after work hours")]
    public async Task SendDisconnectCheckCommand([SlashCommandParameter(Description = "User to check")] User wagie)
    {
        using var scope = _logger.BeginInteractionScope(Context);
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInteractionStart();

        try
        {
            await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());
            DisconnectCheckResult result = await _mediator.Send(new GetDisconnectCheckQuery());

            AttachmentProperties attachment =
                new AttachmentProperties(result.fileResponse.fileName, result.fileResponse.content);

            await Context.Interaction.SendFollowupMessageAsync(
                new InteractionMessageProperties
                {
                    Attachments = new List<AttachmentProperties> { attachment },
                    Content = $"<@{wagie.Id}> — Have you disconnected today? If not, please do so now"
                });

            _logger.LogInteractionSuccess(stopwatch.Elapsed.TotalSeconds);
        }
        catch (Exception e)
        {
            _logger.LogInteractionError(e, stopwatch.Elapsed.TotalSeconds);

            await Context.Interaction.SendFollowupMessageAsync(new InteractionMessageProperties
            {
                Content = "Unexpected error occurred when running the disconnect_check command"
            });
        }
    }
}