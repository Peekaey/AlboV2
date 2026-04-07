
using AlboV2.Features.ScheduledTasks;
using Quartz;

namespace AlboV2.DiscordBot.Startup;

public class ScheduleTaskConfiguration
{
    // https://www.quartz-scheduler.net/documentation/quartz-3.x/tutorial/using-quartz.html#traditional-program-cs
    public static void ConfigureQuartzWithBackgroundJob(WebApplicationBuilder builder)
    {
        builder.Services.AddQuartz(q =>
        {
            var jobKey = new JobKey("RemindEveryoneToDisconnect", "ScheduledReminders");

            q.AddJob<SendRemindEveryoneToDisconnectScheduledTask>(opts => opts
                .WithIdentity(jobKey)
                .WithDescription(
                    "Gets a random clip of Albo and posts in the reminder channel to remind everyone about the right to disconnect"));

            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity("RemindEveryoneToDisconnectTrigger", "ScheduledReminders")

                // Seconds | Minutes | Hours | DayOfMonth | Month| DayOfWeek | Year
                // 5:30PM Monday-Friday AEST
                .WithCronSchedule("0 30 17 ? * MON-FRI", x => x 
                    // .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Australia/Sydney")))
                    .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById(builder.Configuration.GetValue<string>("TimeZoneId"))))
            );
        });
        
        builder.Services.AddQuartzHostedService(opt =>
        {
            opt.WaitForJobsToComplete = false;
        });
    }
    
}