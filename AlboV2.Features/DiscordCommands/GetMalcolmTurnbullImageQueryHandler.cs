using AlboV2.Shared.Models.Dtos;
using MediatR;

namespace AlboV2.Features.DiscordCommands;

public record GetMalcolmTurnbullImageQuery() : IRequest<MalcolmTurnbullImageResult>;

public record MalcolmTurnbullImageResult(FileResponse fileResponse);

public class GetMalcolmTurnbullImageQueryHandler : IRequestHandler<GetMalcolmTurnbullImageQuery, MalcolmTurnbullImageResult>
{
    public GetMalcolmTurnbullImageQueryHandler()
    {
        
    }

    public async Task<MalcolmTurnbullImageResult> Handle(GetMalcolmTurnbullImageQuery request,
        CancellationToken cancellationToken)
    {
        string imagePath = Path.Combine(AppContext.BaseDirectory,  "Assets", "Turnbull", "MalcolmTurnbull.jpg");
        
        if (!File.Exists(imagePath))
        {
            throw new FileNotFoundException("MalcolmTurnbull.jpg", imagePath);
        }

        FileStream fileStream = File.OpenRead(imagePath);
        FileResponse fileResponse = new FileResponse(
            fileStream,
            Path.GetFileName(imagePath),
            "image/jpeg"
            );
        
        return new MalcolmTurnbullImageResult(fileResponse);
    }
}
