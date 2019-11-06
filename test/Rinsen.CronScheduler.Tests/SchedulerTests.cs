using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Rinsen.CronScheduler.Tests
{


    public class SchedulerTests
    {


        [Fact]
        public async Task ScheduleJob()
        {
            var cronDateTimeServiceMock = new Mock<ICronDateTimeService>();
            cronDateTimeServiceMock.Setup(mock => mock.GetNow()).Returns(new DateTime(2019, 11, 06, 14, 43, 59));
            var scheduler = new Scheduler(new CronParser(cronDateTimeServiceMock.Object), cronDateTimeServiceMock.Object, null);

            var actionObject = new ActionObject();
            scheduler.ScheduleTask("* * * * * *", () => {
                actionObject.DoWork();
            });

            using (var cancellationTokenSource = new CancellationTokenSource(1000))
            {
                await scheduler.RunAsync(cancellationTokenSource.Token);
            }

            Assert.True(actionObject.Modified);
        }

        [Fact]
        public async Task ScheduleAsyncJob()
        {
            var cronDateTimeServiceMock = new Mock<ICronDateTimeService>();
            cronDateTimeServiceMock.Setup(mock => mock.GetNow()).Returns(new DateTime(2019, 11, 06, 14, 43, 59));
            var scheduler = new Scheduler(new CronParser(cronDateTimeServiceMock.Object), cronDateTimeServiceMock.Object, null);

            var actionObject = new ActionObject();
            scheduler.ScheduleTask("* * * * * *", async () => {
                await actionObject.DoWorkAsync();
            });

            using (var cancellationTokenSource = new CancellationTokenSource(1000))
            {
                await scheduler.RunAsync(cancellationTokenSource.Token);
            }

            Assert.True(actionObject.Modified);
        }

        [Fact]
        public async Task ScheduleMultipleJobs()
        {
            var cronDateTimeServiceMock = new Mock<ICronDateTimeService>();
            cronDateTimeServiceMock.Setup(mock => mock.GetNow()).Returns(new DateTime(2019, 11, 06, 14, 43, 59));
            var scheduler = new Scheduler(new CronParser(cronDateTimeServiceMock.Object), cronDateTimeServiceMock.Object, null);

            var actionObject = new ActionObject();
            scheduler.ScheduleTask("* * * * * *", () => {
                actionObject.DoWork();
            });

            var actionObject2 = new ActionObject();
            scheduler.ScheduleTask("* * * * * *", () => {
                actionObject2.DoWork();
            });

            var actionObject3 = new ActionObject();
            scheduler.ScheduleTask("* * * * * *", async () => {
                await actionObject3.DoWorkAsync();
            });

            using (var cancellationTokenSource = new CancellationTokenSource(1000))
            {
                await scheduler.RunAsync(cancellationTokenSource.Token);
            }

            Assert.True(actionObject.Modified);
            Assert.True(actionObject2.Modified);
            Assert.True(actionObject3.Modified);
        }
    }

    public class ActionObject
    {
        public bool Modified { get; private set; }

        public string CronExpression { get; set; }

        public void DoWork()
        {
            Modified = true;
        }

        public Task DoWorkAsync()
        {
            Modified = true;

            return Task.CompletedTask;
        }
    }
}
