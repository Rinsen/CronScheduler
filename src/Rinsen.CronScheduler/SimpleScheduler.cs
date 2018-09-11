using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rinsen.CronScheduler
{
    public class SimpleScheduler
    {
        private readonly CronParser _cronParser;

        public SimpleScheduler(CronParser cronParser)
        {
            _cronParser = cronParser;
        }


        public Task WaitForMatch(string expression, CancellationToken cancellationToken)
        {
            var cronExpression = _cronParser.Parse(expression);

            var timeToNextMatch = cronExpression.GetTimeToNext();

            return Task.Delay(timeToNextMatch, cancellationToken);
        }

        public Task WaitForMatch(string expression)
        {
            var cronExpression = _cronParser.Parse(expression);

            var timeToNextMatch = cronExpression.GetTimeToNext();

            return Task.Delay(timeToNextMatch);
        }
    }
}
