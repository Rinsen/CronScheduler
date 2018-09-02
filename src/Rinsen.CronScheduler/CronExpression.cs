using System;
using System.Collections.Generic;

namespace Rinsen.CronScheduler
{
    public class CronExpression
    {
        private readonly ICronDateTimeService _cronDateTimeService;
        private readonly IEnumerable<int> minutes;
        private readonly IEnumerable<int> hours;
        private readonly IEnumerable<int> daysOfMonth;
        private readonly IEnumerable<int> months;
        private readonly IEnumerable<int> dayOfWeek;
        private readonly IEnumerable<int> years;

        public CronExpression(ICronDateTimeService cronDateTimeService,
            IEnumerable<int> minutes,
            IEnumerable<int> hours,
            IEnumerable<int> daysOfMonth,
            IEnumerable<int> months,
            IEnumerable<int> dayOfWeek,
            IEnumerable<int> years)
        {
            _cronDateTimeService = cronDateTimeService;
            this.minutes = minutes;
            this.hours = hours;
            this.daysOfMonth = daysOfMonth;
            this.months = months;
            this.dayOfWeek = dayOfWeek;
            this.years = years;
        }

        public string Minutes { get; }

        public string Hours { get; }

        public string DayOfMonth { get; }

        public string Month { get; }

        public string DayOfWeek { get; }

        public string Year { get; }

        public TimeSpan GetTimeToNext()
        {

            throw new NotImplementedException();
        }


    }
}
