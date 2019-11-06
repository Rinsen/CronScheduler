using System;

namespace Rinsen.CronScheduler
{
    public class CronDateTimeService : ICronDateTimeService
    {
        public DateTime GetNow()
        {
            return DateTime.Now;
        }
    }
}