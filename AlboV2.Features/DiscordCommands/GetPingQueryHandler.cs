using MediatR;

namespace AlboV2.Features.DiscordCommands;

public record GetPingQuery(): IRequest<PingResult>;

public record PingResult(string Message);

public class GetPingQueryHandler : IRequestHandler<GetPingQuery, PingResult>
{
    public GetPingQueryHandler()
    {

    }

    public Task<PingResult> Handle(GetPingQuery request, CancellationToken cancellationToken)
    {
        {
            var result = new PingResult("Pong!");
            return Task.FromResult(result);
        }

    }
}