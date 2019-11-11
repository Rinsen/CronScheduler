using Microsoft.Extensions.Logging;
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
        public async Task NoScheduledJob()
        {
            var cronDateTimeServiceMock = new Mock<ICronDateTimeService>();
            cronDateTimeServiceMock.Setup(mock => mock.GetNow()).Returns(new DateTime(2019, 11, 06, 14, 43, 59));
            var scheduler = new Scheduler(new CronParser(cronDateTimeServiceMock.Object), cronDateTimeServiceMock.Object, null);

            using (var cancellationTokenSource = new CancellationTokenSource(1000))
            {
                await scheduler.RunAsync(cancellationTokenSource.Token);
            }
        }

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
            scheduler.ScheduleTask("* * * * *", () => {
                actionObject2.DoWork();
            });

            var actionObject3 = new ActionObject();
            scheduler.ScheduleTask("* * * * *", async () => {
                await actionObject3.DoWorkAsync();
            });

            using (var cancellationTokenSource = new CancellationTokenSource(1100))
            {
                await scheduler.RunAsync(cancellationTokenSource.Token);
            }

            Assert.True(actionObject.Modified);
            Assert.True(actionObject2.Modified);
            Assert.True(actionObject3.Modified);
        }

        [Fact]
        public async Task ScheduleJobButLastRunTimeIsInPast()
        {
            var cronDateTimeServiceMock = new Mock<ICronDateTimeService>();
            cronDateTimeServiceMock.Setup(mock => mock.GetNow()).Returns(new DateTime(2019, 11, 06, 14, 43, 59));
            var scheduler = new Scheduler(new CronParser(cronDateTimeServiceMock.Object), cronDateTimeServiceMock.Object, null);

            var actionObject = new ActionObject();
            scheduler.ScheduleTask("* * * * * 2018", () => {
                actionObject.DoWork();
            });

            using (var cancellationTokenSource = new CancellationTokenSource(1000))
            {
                await scheduler.RunAsync(cancellationTokenSource.Token);
            }

            Assert.False(actionObject.Modified);
        }

        [Fact]
        public async Task ScheduleJobThatShouldRunInNextMinuteAndNotMore()
        {
            var cronDateTimeServiceMock = new Mock<ICronDateTimeService>();
            cronDateTimeServiceMock.SetupSequence(mock => mock.GetNow())
                .Returns(new DateTime(2019, 11, 06, 14, 43, 58))
                .Returns(new DateTime(2019, 11, 06, 14, 43, 59))
                .Returns(new DateTime(2019, 11, 06, 14, 44, 01));
            var scheduler = new Scheduler(new CronParser(cronDateTimeServiceMock.Object), cronDateTimeServiceMock.Object, null);

            var actionObject = new ActionObject();
            scheduler.ScheduleTask("44 14 * * * 2019", () => {
                actionObject.DoWork();
            });

            using (var cancellationTokenSource = new CancellationTokenSource(1100))
            {
                await scheduler.RunAsync(cancellationTokenSource.Token);
            }
            
            Assert.Equal(1, actionObject.ModifiedCount);
        }

        [Fact]
        public async Task ScheduleJobThatShouldRunInNextMinuteNutCancelBeforeThatSoNoExecutionHaveOccured()
        {
            var cronDateTimeServiceMock = new Mock<ICronDateTimeService>();
            cronDateTimeServiceMock.SetupSequence(mock => mock.GetNow())
                .Returns(new DateTime(2019, 11, 06, 14, 43, 48))
                .Returns(new DateTime(2019, 11, 06, 14, 43, 49));
            var scheduler = new Scheduler(new CronParser(cronDateTimeServiceMock.Object), cronDateTimeServiceMock.Object, null);

            var actionObject = new ActionObject();
            scheduler.ScheduleTask("44 14 * * * 2019", () => {
                actionObject.DoWork();
            });

            using (var cancellationTokenSource = new CancellationTokenSource(1))
            {
                await scheduler.RunAsync(cancellationTokenSource.Token);
            }

            Assert.False(actionObject.Modified);
        }

        [Fact]
        public async Task ScheduleJobThatWillTakeMoreThanAMinuteToRunAndLogWarning()
        {
            var cronDateTimeServiceMock = new Mock<ICronDateTimeService>();
            cronDateTimeServiceMock.SetupSequence(mock => mock.GetNow())
                .Returns(new DateTime(2019, 11, 06, 14, 43, 58))
                .Returns(new DateTime(2019, 11, 06, 14, 43, 59))
                .Returns(new DateTime(2019, 11, 06, 14, 44, 01))
                .Returns(new DateTime(2019, 11, 06, 14, 45, 02));

            var logger = new Mock<ILogger<Scheduler>>();

            var scheduler = new Scheduler(new CronParser(cronDateTimeServiceMock.Object), cronDateTimeServiceMock.Object, logger.Object);

            var actionObject = new ActionObject();
            scheduler.ScheduleTask("44 14 * * * 2019", () => {
                actionObject.DoWork();
            });

            using (var cancellationTokenSource = new CancellationTokenSource(2000))
            {
                await scheduler.RunAsync(cancellationTokenSource.Token);
            }

            logger.Verify(x => x.Log(LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => string.Equals("Execution took more than one minute", o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }
    }

    public class ActionObject
    {
        public bool Modified { get { return ModifiedCount > 0; } }

        public int ModifiedCount { get; private set; }

        public string CronExpression { get; set; }

        public void DoWork()
        {
            ModifiedCount++;
        }

        public Task DoWorkAsync()
        {
            ModifiedCount++;

            return Task.CompletedTask;
        }
    }
}
