using System.Diagnostics;
using AlboV2.Features.DiscordCommands;
using AlboV2.Shared.Helpers;
using AlboV2.Shared.Service;
using Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quartz;

namespace AlboV2.Features.ScheduledTasks;

[DisallowConcurrentExecution]
public class SendRemindEveryoneToDisconnectScheduledTask : IJob
{
    private readonly ILogger<SendRemindEveryoneToDisconnectScheduledTask> _logger;
    private readonly IMediator _mediator;
    private readonly ISendRemindEveryoneToDisconnectScheduledMessage _sendRemindEveryoneToDisconnectScheduledMessage;
    private readonly IConfiguration _configuration;
    private readonly IDateTimeHelperService _dateTimeHelperService;
    public SendRemindEveryoneToDisconnectScheduledTask(ILogger<SendRemindEveryoneToDisconnectScheduledTask> logger, IMediator mediator,
        ISendRemindEveryoneToDisconnectScheduledMessage sendRemindEveryoneToDisconnectScheduledMessage, IConfiguration configuration,
        IDateTimeHelperService dateTimeHelperService)
    {
        _logger = logger;
        _mediator = mediator;
        _sendRemindEveryoneToDisconnectScheduledMessage = sendRemindEveryoneToDisconnectScheduledMessage;
        _configuration = configuration;
        _dateTimeHelperService = dateTimeHelperService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogScheduledTaskStart();

        var isPublicHoliday = await _dateTimeHelperService.IsDatePublicHoliday(DateTime.UtcNow, _configuration["TimezoneId"]);

        if (isPublicHoliday)
        {
            _logger.LogInformation("Today is a public holiday. Skipping the right to disconnect reminder...");
            _logger.LogScheduledTaskSuccess(stopwatch.Elapsed.TotalSeconds);
            return;
        }

        try
        {
            RemindEveryoneToDisconnectResult result = await _mediator.Send(new GetRemindEveryoneToDisconnectQuery());
            await _sendRemindEveryoneToDisconnectScheduledMessage.SendRemindEveryoneToDisconnectScheduledMessage(result);
            _logger.LogScheduledTaskSuccess(stopwatch.Elapsed.TotalSeconds);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "SendRemindEveryoneToDisconnectScheduledMessage failed");
            _logger.LogScheduledTaskError(e, stopwatch.Elapsed.TotalSeconds);
        }
    }
    
}