using ChatApp.Extensions;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Quartz.Impl;
using TWChatAppApiMaster.Databases.ChatApp;

namespace TWChatAppApiMaster.Timers
{
    public class AutoDeleteMessageJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            var _context = ServiceExtension.GetDbContext();
            try
            {
                
            }
            catch (Exception ex)
            {

            }
            finally { 
                _context.Dispose();
            }

            return Task.CompletedTask;
        }
    }

    public class JobScheduler
    {

  
        public static void Start()

        {
            IScheduler scheduler = (IScheduler)StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();

            IJobDetail job = JobBuilder.Create<AutoDeleteMessageJob>().Build();
            ITrigger trigger = TriggerBuilder.Create()
                //.WithDailyTimeIntervalSchedule
                //  (s =>
                //     s.WithIntervalInHours(24)
                //    .OnEveryDay()
                //    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(16, 58))
                //  )
                //.Build();
                .WithSimpleSchedule
                  (s =>
                     s.WithIntervalInMinutes(1)
                  )
                .Build();

            scheduler.ScheduleJob(job, trigger);

        }
    }


}
