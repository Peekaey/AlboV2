using System.Diagnostics;
using AlboV2.Features.DiscordCommands;
using AlboV2.Shared.Helpers;
using MediatR;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace AlboV2.DiscordBot.DiscordCommands;

public class PingDiscordCommand : ApplicationCommandModule<ApplicationCommandContext>
{
    private readonly ILogger<PingDiscordCommand> _logger;
    private readonly IMediator _mediator;

    public PingDiscordCommand(ILogger<PingDiscordCommand> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [SlashCommand("ping", "replies with pong!")]
    public async Task SendPingAsync()
    {
        using var scope = _logger.BeginInteractionScope(Context);
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInteractionStart();

        try
        {
            await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());
            
            PingResult result = await _mediator.Send(new GetPingQuery());
            
            await Context.Interaction.SendFollowupMessageAsync(new InteractionMessageProperties
            {
                Content = result.Message
            });

            _logger.LogInteractionSuccess(stopwatch.Elapsed.TotalSeconds);
        }
        catch (Exception e)
        {
            _logger.LogInteractionError(e, stopwatch.Elapsed.TotalSeconds);
            
            await Context.Interaction.SendFollowupMessageAsync(new InteractionMessageProperties
            {
                Content = "Unexpected error occurred when running the ping command."
            });

        }
    }
}