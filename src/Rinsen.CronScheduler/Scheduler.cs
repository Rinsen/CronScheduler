using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rinsen.CronScheduler
{
    public class Scheduler
    {
        private readonly List<ScheduledTask> _scheduledTasks = new List<ScheduledTask>();
        private readonly CronParser _cronParser;
        private readonly ICronDateTimeService _cronDateTimeService;
        private readonly ILogger<Scheduler> _logger;

        public Scheduler(CronParser cronParser,
            ICronDateTimeService cronDateTimeService,
            ILogger<Scheduler> logger)
        {
            _cronParser = cronParser;
            _cronDateTimeService = cronDateTimeService;
            _logger = logger;
        }

        public ScheduledTask ScheduleTask(string cronExpression, Func<Task> func)
        {
            var expression = _cronParser.Parse(cronExpression);

            var scheduledTask = new ScheduledTask(expression, func);

            _scheduledTasks.Add(scheduledTask);

            return scheduledTask;
        }

        public ScheduledTask ScheduleTask(string cronExpression, Action action)
        {
            var expression = _cronParser.Parse(cronExpression);

            var scheduledTask = new ScheduledTask(expression, action);

            _scheduledTasks.Add(scheduledTask);

            return scheduledTask;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            List<Guid> lowestIds = new List<Guid>();
            DateTime lowestNextTimeToRun = DateTime.MaxValue;

            while (!cancellationToken.IsCancellationRequested)
            {
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
                var timeToWait = lowestNextTimeToRun.Subtract(_cronDateTimeService.GetNow());

                await Task.Delay((int)timeToWait.TotalMilliseconds, cancellationToken).ContinueWith(task => {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                });

                foreach (var scheduledTask in _scheduledTasks.Where(m => lowestIds.Contains(m.Id)))
                {
                    try
                    {
                        if (scheduledTask.Action is object)
                        {
                            scheduledTask.Action.Invoke();
                        }

                        if (scheduledTask.ActionTask is object)
                        {
                            await scheduledTask.ActionTask.Invoke();
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Failed to execute action");
                    }
                }
            }
        }
    }
}
