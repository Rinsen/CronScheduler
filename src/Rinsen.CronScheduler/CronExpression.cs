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
            // If no changes is required this is the next time of execution
            var next = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
            
            // Find the year of next execution
            if (_years.Any())
            {
                var years = _years.Where(m => m >= now.Year).ToArray();

                if (!years.Any())
                {
                    return null;
                }
                else if (next.Year != years.First())
                {
                    next = new DateTime(years.First(), 1, 1);
                }
            }

            if (_months.Any())
            {
                var months = _months.Where(m => m > now.Month).ToArray();

                int fullMonthsToNext;
                if (!months.Any())
                {
                    fullMonthsToNext = 11 - next.Month + _months.First();
                }
                else
                {
                    fullMonthsToNext = months.First() - next.Month - 1;
                }
                next = next.AddMonths(fullMonthsToNext);

                var fullDaysToNext = 0;
                while (next.AddDays(fullDaysToNext).Day != 1)
                {
                    fullDaysToNext++;
                }
                fullDaysToNext--;
                next = next.AddDays(fullDaysToNext);
                next = next.AddHours(23 - next.Hour);
                next = next.AddMinutes(60 - next.Minute);
            }

            if (_daysOfMonth.Any())
            {
                var daysOfMonth = _daysOfMonth.Where(m => m > now.Day).ToArray();

                var fullDaysToNext = 0;
                if (!daysOfMonth.Any())
                {
                    var nextDay = _daysOfMonth.First();
                    while (next.AddDays(fullDaysToNext).Day != nextDay)
                    {
                        fullDaysToNext++;
                    }
                    fullDaysToNext--;
                }
                else
                {
                    fullDaysToNext = daysOfMonth.First() - next.Day - 1;
                }
                next = next.AddDays(fullDaysToNext);
                next = next.AddHours(23 - next.Hour);
                next = next.AddMinutes(60 - next.Minute);
            }

            if (_hours.Any())
            {
                var hours = _hours.Where(m => m >= now.Hour).ToArray();

                int fullHoursToNext;
                if (!hours.Any())
                {
                    fullHoursToNext = 23 - next.Hour + _hours.First();
                }
                else
                {
                    fullHoursToNext = hours.First() - next.Hour - 1;
                }

                next = next.AddHours(fullHoursToNext);
                next = next.AddMinutes(60 - next.Minute);
            }

            if (_minutes.Any())
            {
                var minutes = _minutes.Where(m => m >= now.Minute).ToArray();

                if (!minutes.Any())
                {
                    next = next.AddMinutes(60 - now.Minute + _minutes.First());
                }
                else if (next.Minute != minutes.First())
                {
                    next = new DateTime(next.Year, next.Month, next.Day, next.Hour, minutes.First(), 0);
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
