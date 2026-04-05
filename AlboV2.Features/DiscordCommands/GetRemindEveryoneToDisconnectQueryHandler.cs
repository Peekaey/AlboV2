using AlboV2.Shared.Helpers;
using AlboV2.Shared.Models.Dtos;
using Mediator;

namespace AlboV2.Features.DiscordCommands;


public record GetRemindEveryoneToDisconnectQuery() : IRequest<RemindEveryoneToDisconnectResult>;

public record RemindEveryoneToDisconnectResult(FileResponse fileResponse);

public class GetRemindEveryoneToDisconnectQueryHandler : IRequestHandler<GetRemindEveryoneToDisconnectQuery, RemindEveryoneToDisconnectResult>
{
    public GetRemindEveryoneToDisconnectQueryHandler()
    {
        
    }

    public ValueTask<RemindEveryoneToDisconnectResult> Handle(GetRemindEveryoneToDisconnectQuery request,
        CancellationToken cancellationToken)
    {
        string fileName = ImageHelperExtensions.GetRngAlboFilename();
        string imagePath= Path.Combine(AppContext.BaseDirectory, "Assets", "Albo", fileName);

        if (!File.Exists(imagePath))
        {
            throw new FileNotFoundException("albo.mov", imagePath);
        }
        
        FileStream fileStream = File.OpenRead(imagePath);
        FileResponse fileResponse = new FileResponse(
            fileStream,
            Path.GetFileName(imagePath),
            "video/quicktime"
        );

        return ValueTask.FromResult(new RemindEveryoneToDisconnectResult(fileResponse));
        
    }
}