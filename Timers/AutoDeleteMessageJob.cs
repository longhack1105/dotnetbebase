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
                var lstAutoDelete = _context.RegisterAutoDelete.Where(x=> x.PeriodTime > 0).ToList();
                foreach (var item in lstAutoDelete)
                {
                    double actualPeriod = (DateTime.Now - (DateTime)item.LastTimeDelete).TotalDays;
                    if(actualPeriod >= item.PeriodTime)
                    {
                        var room = _context.Rooms.AsNoTracking()
                            .Include(x => x.LastMessageUu)
                            .Where(x => x.Uuid == item.RoomUuid)
                            .SingleOrDefault();
                        if (room != null)
                        {
                            var messageLst = _context.Messages
                                .AsNoTracking()
                                .Where(x => x.RoomUuid == room.Uuid)
                                .Where(x => !x.MessageDelete.Any(x => x.UserName == item.UserName))
                                .Where(x => room.LastMessageUu == null || x.Id <= room.LastMessageUu.Id)
                                .ToList();

                            var newDelete = messageLst
                                .Select(x => new MessageDelete
                                {
                                    MessageUuid = x.Uuid,
                                    UserName = item.UserName,
                                })
                                .ToList();

                            _context.MessageDelete.AddRange(newDelete);
                            item.LastTimeDelete = DateTime.Now;

                            _context.SaveChanges();
                        }
                    }
                }
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
