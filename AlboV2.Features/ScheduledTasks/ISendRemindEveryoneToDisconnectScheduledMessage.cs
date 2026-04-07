using AlboV2.Features.DiscordCommands;

namespace AlboV2.Features.ScheduledTasks;

public interface ISendRemindEveryoneToDisconnectScheduledMessage
{
    Task SendRemindEveryoneToDisconnectScheduledMessage(RemindEveryoneToDisconnectResult result);
}