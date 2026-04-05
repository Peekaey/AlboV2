using Mediator;

namespace AlboV2.Features.DiscordCommands;

public record GetPingQuery(): IRequest<PingResult>;

public record PingResult(string Message);

public class GetPingQueryHandler : IRequestHandler<GetPingQuery, PingResult>
{
    public GetPingQueryHandler()
    {

    }

    public async ValueTask<PingResult> Handle(GetPingQuery request, CancellationToken cancellationToken)
    {
            return new PingResult("Pong!");
    }
}