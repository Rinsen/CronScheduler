using System;
using System.Collections.Generic;
using System.Text;

namespace Rinsen.CronScheduler
{
    public class ScheduleItem
    {
        public Guid Id { get; set; }

        public string CronExpression { get; set; }

    }
}
