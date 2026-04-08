using AlboV2.Shared.Helpers;
using AlboV2.Shared.Models.Dtos;
using Mediator;

namespace AlboV2.Features.DiscordCommands;

public record GetDisconnectCheckQuery(): IRequest<DisconnectCheckResult>;

public record DisconnectCheckResult(FileResponse fileResponse);

public class GetDisconnectCheckQueryHandler : IRequestHandler<GetDisconnectCheckQuery, DisconnectCheckResult>
{
    public GetDisconnectCheckQueryHandler()
    {
        
    }

    public ValueTask<DisconnectCheckResult> Handle(GetDisconnectCheckQuery request, CancellationToken cancellationToken)
    {
        string fileName = ImageHelpers.GetRngAlboFilename();
        string imagePath= Path.Combine(AppContext.BaseDirectory, "Assets", "Albo", fileName);

        if (!File.Exists(imagePath))
        {
            throw new FileNotFoundException(fileName, imagePath);
        }
        
        FileStream fileStream = File.OpenRead(imagePath);
        FileResponse fileResponse = new FileResponse(
            fileStream,
            Path.GetFileName(imagePath),
            "video/quicktime"
        );

        return ValueTask.FromResult(new DisconnectCheckResult(fileResponse));
    }
}