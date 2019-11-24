using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rinsen.CronScheduler
{
    public class ScheduledTask
    {

        public ScheduledTask(CronExpression cronExpression, Action<CancellationToken> action)
        {
            CronExpression = cronExpression;
            Action = action;
        }

        public ScheduledTask(CronExpression cronExpression, Func<CancellationToken, Task> action)
        {
            CronExpression = cronExpression;
            ActionTask = action;
        }

        public void SetCronExpression(CronExpression cronExpression)
        {
            CronExpression = cronExpression;
        }

        public Guid Id { get; } = Guid.NewGuid();
        public CronExpression CronExpression { get; private set; }
        public Action<CancellationToken> Action { get; }
        public Func<CancellationToken, Task> ActionTask { get; }
    }
}
