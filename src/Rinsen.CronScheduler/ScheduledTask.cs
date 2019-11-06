using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rinsen.CronScheduler
{
    public class ScheduledTask
    {

        public ScheduledTask(CronExpression cronExpression, Action action)
        {
            CronExpression = cronExpression;
            Action = action;
        }

        public ScheduledTask(CronExpression cronExpression, Func<Task> action)
        {
            CronExpression = cronExpression;
            ActionTask = action;
        }

        public Guid Id { get; } = Guid.NewGuid();
        public CronExpression CronExpression { get; }
        public Action Action { get; }
        public Func<Task> ActionTask { get; }
    }
}
