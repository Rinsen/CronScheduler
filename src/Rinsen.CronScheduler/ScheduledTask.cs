using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rinsen.CronScheduler
{
    public class ScheduledTask
    {

        public ScheduledTask(Guid id, CronExpression cronExpression, Action<CancellationToken> action)
        {
            Id = id;
            CronExpression = cronExpression;
            Action = action;
        }

        public ScheduledTask(Guid id, CronExpression cronExpression, Func<CancellationToken, Task> action)
        {
            Id = id;
            CronExpression = cronExpression;
            ActionTask = action;
        }

        internal void SetCronExpression(CronExpression cronExpression)
        {
            CronExpression = cronExpression;
        }

        public Guid Id { get; }
        public CronExpression CronExpression { get; private set; }
        internal Action<CancellationToken> Action { get; }
        internal Func<CancellationToken, Task> ActionTask { get; }
    }
}
