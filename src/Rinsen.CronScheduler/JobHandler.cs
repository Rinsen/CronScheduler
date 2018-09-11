using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rinsen.CronScheduler
{
    public class JobHandler
    {
        public Guid JobId { get; }

        private Type _type;
        private ICronJob _cronJob;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private Task _runningTask;

        private CancellationTokenSource _cancellationTokenSource;

        private CronExpression _cronExpression;
        private CronParser _cronParser;

        public JobHandler(Guid jobId, ICronJob cronJob, string cronExpression, IServiceScopeFactory serviceScopeFactory)
        {
            JobId = jobId;
            _cronJob = cronJob;
            _serviceScopeFactory = serviceScopeFactory;
            _cronParser = new CronParser(new CronDateTimeService());
            _cronExpression = _cronParser.Parse(cronExpression);
        }

        public void Create<T>() where T : ICronJob
        {
            _type = typeof(T);

            _cancellationTokenSource = new CancellationTokenSource();

            _runningTask = Task.Run(async () =>
            {
                await RunCronJob<T>();
            }
            , _cancellationTokenSource.Token);
        }

        private async Task RunCronJob<T>() where T : ICronJob
        {
            using (var serviceScope = _serviceScopeFactory.CreateScope())
            {
                _cronJob = serviceScope.ServiceProvider.GetService<T>();

                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    await Task.Delay(_cronExpression.GetTimeToNext(), _cancellationTokenSource.Token);

                    await _cronJob.RunJob(_cancellationTokenSource.Token);
                }
            }
        }

        public void ChangeSchedule<T>(string cronString) where T : ICronJob
        {
            Stop();

            _cronExpression = _cronParser.Parse(cronString);

            Create<T>();
        }


        public void Stop()
        {
            _cancellationTokenSource.Cancel();

            _runningTask.Wait(); // Really safe to do this??!!

            _cancellationTokenSource.Dispose();
        }


    }
}
