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

        public TimeSpan? GetTimeToNext()
        {
            var now = _cronDateTimeService.GetNow();

            if(ShouldRunNow(now)) // Now is not the next time to run
            {
                // Complicated shit



            }
            else
            {
                // Complicated shit


            }

            return null;
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
