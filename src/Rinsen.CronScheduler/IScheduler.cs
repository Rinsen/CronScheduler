using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rinsen.CronScheduler
{
    public interface IScheduler
    {
        void ChangeScheduleAndResetScheduler(Guid id, string cronExpression);
        void ChangeSchedulesAndResetScheduler(IEnumerable<ScheduleItem> scheduleChanges);
        Task RunAsync(CancellationToken cancellationToken);
        Guid ScheduleTask(string cronExpression, Action<CancellationToken> action);
        void ScheduleTask(Guid id, string cronExpression, Action<CancellationToken> action);
        Guid ScheduleTask(string cronExpression, Func<CancellationToken, Task> func);
        void ScheduleTask(Guid id, string cronExpression, Func<CancellationToken, Task> func);
        void Stop();
    }
}