using System;

namespace Rinsen.CronScheduler
{
    public interface ICronDateTimeService
    {
        DateTime GetNow();
    }
}
