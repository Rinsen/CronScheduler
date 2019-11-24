using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rinsen.CronScheduler
{
    public class Scheduler : IScheduler
    {
        private readonly List<ScheduledTask> _scheduledTasks = new List<ScheduledTask>();
        private readonly CronParser _cronParser;
        private readonly ICronDateTimeService _cronDateTimeService;
        private readonly ILogger<Scheduler> _logger;
        private readonly TimeSpan MINUTE = new TimeSpan(0, 1, 0);
        private CancellationTokenSource _localCancellationTokenSource = new CancellationTokenSource();
        private CancellationToken _externalCancellationToken;

        public Scheduler(CronParser cronParser,
            ICronDateTimeService cronDateTimeService,
            ILogger<Scheduler> logger)
        {
            _cronParser = cronParser;
            _cronDateTimeService = cronDateTimeService;
            _logger = logger;
        }

        public void ScheduleTask(Guid id, string cronExpression, Func<CancellationToken, Task> func)
        {
            var expression = _cronParser.Parse(cronExpression);

            var scheduledTask = new ScheduledTask(id, expression, func);

            _scheduledTasks.Add(scheduledTask);
        }
        public Guid ScheduleTask(string cronExpression, Func<CancellationToken, Task> func)
        {
            var id = Guid.NewGuid();

            ScheduleTask(id, cronExpression, func);

            return id;
        }

        public void ScheduleTask(Guid id, string cronExpression, Action<CancellationToken> action)
        {
            var expression = _cronParser.Parse(cronExpression);

            var scheduledTask = new ScheduledTask(id, expression, action);

            _scheduledTasks.Add(scheduledTask);
        }

        public Guid ScheduleTask(string cronExpression, Action<CancellationToken> action)
        {
            var id = Guid.NewGuid();

            ScheduleTask(id, cronExpression, action);

            return id;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            _externalCancellationToken = cancellationToken;
            _externalCancellationToken.Register(() => _localCancellationTokenSource.Cancel());
            List<Guid> lowestIds = new List<Guid>();

            while (!cancellationToken.IsCancellationRequested)
            {
                var lowestNextTimeToRun = GetScheduledTasksToRunAndHowLongToWait(lowestIds);

                var timeToWait = lowestNextTimeToRun.Subtract(_cronDateTimeService.GetNow());

                var continueExecution = await Task.Delay((int)Math.Min(int.MaxValue, timeToWait.TotalMilliseconds), _localCancellationTokenSource.Token).ContinueWith(task =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return false;
                    }
                    return true;
                });

                if (!continueExecution)
                    return;

                var startTime = _cronDateTimeService.GetNow();
                foreach (var scheduledTask in _scheduledTasks.Where(m => lowestIds.Contains(m.Id)))
                {
                    if (_localCancellationTokenSource.IsCancellationRequested)
                        break;

                    try
                    {
                        if (scheduledTask.Action is object)
                        {
                            scheduledTask.Action.Invoke(_localCancellationTokenSource.Token);
                        }

                        if (scheduledTask.ActionTask is object)
                        {
                            await scheduledTask.ActionTask.Invoke(_localCancellationTokenSource.Token);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Failed to execute action");
                    }
                }
                if (_cronDateTimeService.GetNow() - startTime > MINUTE)
                {
                    _logger.LogWarning("Execution took more than one minute");
                }
            }
        }

        private DateTime GetScheduledTasksToRunAndHowLongToWait(List<Guid> lowestIds)
        {
            var lowestNextTimeToRun = DateTime.MaxValue;
            lowestIds.Clear();

            foreach (var scheduledTask in _scheduledTasks)
            {
                var nextTimeToRun = scheduledTask.CronExpression.GetNextTimeToRun();

                if (nextTimeToRun == default)
                    continue;

                if (nextTimeToRun < lowestNextTimeToRun)
                {
                    lowestIds.Clear();
                    lowestIds.Add(scheduledTask.Id);
                    lowestNextTimeToRun = (DateTime)nextTimeToRun;
                }
                else if (nextTimeToRun == lowestNextTimeToRun)
                {
                    lowestIds.Add(scheduledTask.Id);
                }
            }

            return lowestNextTimeToRun;
        }

        private void RegisterLocalCancelationToken()
        {
            _localCancellationTokenSource = new CancellationTokenSource();
            _externalCancellationToken.Register(() => _localCancellationTokenSource.Cancel());
        }

        public void ChangeScheduleAndResetScheduler(Guid id, string cronExpression)
        {
            _scheduledTasks.Single(t => t.Id == id).SetCronExpression(_cronParser.Parse(cronExpression));

            ResetScheduler();
        }

        public void ChangeSchedulesAndResetScheduler(IEnumerable<ScheduleItem> scheduleChanges)
        {
            foreach (var scheduleItem in scheduleChanges)
            {
                _scheduledTasks.Single(t => t.Id == scheduleItem.Id).SetCronExpression(_cronParser.Parse(scheduleItem.CronExpression));
            }

            ResetScheduler();
        }

        private void ResetScheduler()
        {
            _localCancellationTokenSource.Cancel();

            RegisterLocalCancelationToken();
        }

        public void Stop()
        {
            _localCancellationTokenSource.Cancel();
        }
    }
}
