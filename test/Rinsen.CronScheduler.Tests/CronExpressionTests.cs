using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Rinsen.CronScheduler.Tests
{
    public class CronExpressionTests
    {


        [Fact]
        public void When_Then()
        {
            var cronDateTimeServiceMock = new Mock<ICronDateTimeService>();
            cronDateTimeServiceMock.Setup(m => m.GetNow()).Returns(new DateTime(2018, 09, 11, 23, 00, 01));

            var cronParser = new CronParser(cronDateTimeServiceMock.Object);
            var cronExpression = cronParser.Parse("0 * * * *");
            var cronExpression2 = cronParser.Parse("1 * * * *");

            Assert.True(cronExpression.ShouldRunNow());
            Assert.False(cronExpression2.ShouldRunNow());


            

        }

    }
}
