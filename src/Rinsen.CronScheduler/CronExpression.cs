using System;
using System.Collections.Generic;
using System.Linq;

namespace Rinsen.CronScheduler
{
    public class CronExpression
    {
        private readonly ICronDateTimeService _cronDateTimeService;
        private readonly IReadOnlyCollection<int> _minutes;
        private readonly IReadOnlyCollection<int> _hours;
        private readonly IReadOnlyCollection<int> _daysOfMonth;
        private readonly IReadOnlyCollection<int> _months;
        private readonly IReadOnlyCollection<int> _dayOfWeek;
        private readonly IReadOnlyCollection<int> _years;

        public CronExpression(ICronDateTimeService cronDateTimeService,
            IReadOnlyCollection<int> minutes,
            IReadOnlyCollection<int> hours,
            IReadOnlyCollection<int> daysOfMonth,
            IReadOnlyCollection<int> months,
            IReadOnlyCollection<int> dayOfWeek,
            IReadOnlyCollection<int> years)
        {
            _cronDateTimeService = cronDateTimeService;
            _minutes = minutes;
            _hours = hours;
            _daysOfMonth = daysOfMonth;
            _months = months;
            _dayOfWeek = dayOfWeek;
            _years = years;
        }

        public string Minutes { get { return GetTimeString(_minutes); } }

        public string Hours { get { return GetTimeString(_hours); } }
        
        public string DayOfMonth { get { return GetTimeString(_daysOfMonth); } }
        
        public string Month { get { return GetTimeString(_months); } }
        
        public string DayOfWeek { get { return GetTimeString(_dayOfWeek); } }
        
        public string Year { get { return GetTimeString(_years); } }
        
        private string GetTimeString(IReadOnlyCollection<int> data)
        {
            if (!data.Any())
            {
                return CronParser.Asterisk;
            }

            return string.Join(",", data);

        }

        public DateTime? GetNextTimeToRun()
        {
            var now = _cronDateTimeService.GetNow();

            return PrivateGetNextTimeToRun(now);
        }

        private DateTime? PrivateGetNextTimeToRun(DateTime now)
        {
            if (ShouldRunNow(now)) // Now is not the next time to run
            {
                // Complicated shit
                var nextMinute = now.AddMinutes(1);

                if (ShouldRunNow(nextMinute))
                {
                    return new DateTime(nextMinute.Year, nextMinute.Month, nextMinute.Day, nextMinute.Hour, nextMinute.Minute, 0);
                }

                var nextMinuteMatch = GetNextMatch(nextMinute);

                return nextMinuteMatch;
            }

            return GetNextMatch(now);
        }

        public TimeSpan? GetTimeToNext()
        {
            var now = _cronDateTimeService.GetNow();

            return PrivateGetNextTimeToRun(now)?.Subtract(now);
        }

        private DateTime? GetNextMatch(DateTime now)
        {
            var next = new DateTime(now.Year, 1, 1, 0, 0, 0);
            var timeUpdated = false;

            // Find the year of next execution
            if (_years.Any())
            {
                var years = _years.Where(m => m > now.Year).ToArray();

                if (!years.Any())
                {
                    return null;
                }
                else
                {
                    next = next.AddYears(years.First() - next.Year);
                }

                timeUpdated = true;
            }

            if (_months.Any())
            {
                var months = _months.Where(m => m > now.Month).ToArray();

                if (!months.Any())
                {
                    next = next.AddYears(1);
                    next = next.AddMonths(_months.First() - 1);
                }
                else
                {
                    next = next.AddMonths(months.First() - 1);
                }

                timeUpdated = true;
            }
            else if (!timeUpdated)
            {
                next = next.AddMonths(now.Month - 1);
            }

            if (_daysOfMonth.Any())
            {
                var daysOfMonth = _daysOfMonth.Where(m => m > now.Day).ToArray();

                if (!daysOfMonth.Any())
                {
                    next = next.AddMonths(1);
                    next = next.AddDays(_daysOfMonth.First() - next.Day);
                }
                else
                {
                    next = next.AddDays(daysOfMonth.First() - next.Day);
                }

                timeUpdated = true;
            }
            else if (!timeUpdated)
            {
                next = next.AddDays(now.Day - 1);
            }

            if (_hours.Any())
            {
                var hours = _hours.Where(m => m > now.Hour).ToArray();

                if (!hours.Any())
                {
                    next = next.AddDays(1);
                    next = next.AddHours(_hours.First());
                }
                else
                {
                    next = next.AddHours(hours.First());
                }
            }
            else if (!timeUpdated)
            {
                next = next.AddHours(now.Hour);
            }

            if (_minutes.Any())
            {
                var minutes = _minutes.Where(m => m > now.Minute).ToArray();

                if (!minutes.Any())
                {
                    next = next.AddHours(1);
                    next = next.AddMinutes(_minutes.First());
                }
                else if (!timeUpdated)
                {
                    next = next.AddMinutes(minutes.First());
                }
            }

            return next;
        }

        public bool ShouldRunNow()
        {
            var now = _cronDateTimeService.GetNow();

            return ShouldRunNow(now);
        }

        private bool ShouldRunNow(DateTime now)
        {
            if (_minutes.Any())
            {
                if (!_minutes.Contains(now.Minute))
                    return false;
            }

            if (_hours.Any())
            {
                if (!_hours.Contains(now.Hour))
                    return false;
            }

            if (_daysOfMonth.Any())
            {
                if (!_daysOfMonth.Contains(now.Day))
                    return false;
            }

            if (_months.Any())
            {
                if (!_hours.Contains(now.Month))
                    return false;
            }

            if (_dayOfWeek.Any())
            {
                if (!_dayOfWeek.Contains((int)now.DayOfWeek))
                    return false;
            }

            if (_years.Any())
            {
                if (!_years.Contains(now.Year))
                    return false;
            }

            return true;
        }
    }
}
