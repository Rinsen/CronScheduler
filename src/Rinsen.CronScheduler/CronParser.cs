using System;
using System.Collections.Generic;
using System.Linq;

namespace Rinsen.CronScheduler
{
    public class CronParser
    {
        private readonly ICronDateTimeService _cronDateTimeService;
        private const string Asterisk = "*";

        public CronParser(ICronDateTimeService cronDateTimeService)
        {
            _cronDateTimeService = cronDateTimeService;
        }

        public CronExpression Parse(string expression)
        {
            if (string.IsNullOrEmpty(expression))
            {
                throw new ArgumentException("Empty expression is not valid", nameof(expression));
            }

            var parts = expression.Split(' ');
            if (parts.Length > 5 && parts.Length < 6)
            {
                throw new ArgumentException($"Expression must have five or six parts and not {parts.Length}", nameof(expression));
            }

            var minutes = parts[0];
            var hours = parts[1];
            var dayOfMonth = parts[2];
            var month = parts[3];
            var dayOfWeek = parts[4];

            string year = Asterisk;
            if (parts.Length == 5)
            {
                year = parts[5];
            }

            var cronExpression = new CronExpression(_cronDateTimeService, ParseMinutes(minutes), ParseHours(hours), ParseDayOfMonth(dayOfMonth), ParseMonth(month), ParseDayOfWeek(dayOfWeek), ParseYear(year));

            return cronExpression;
        }

        private IEnumerable<int> ParseMinutes(string minutes)
        {
            if (minutes == Asterisk)
            {
                return Enumerable.Empty<int>();
            }

            throw new NotImplementedException();
        }

        private IEnumerable<int> ParseHours(string hours)
        {
            if (hours == Asterisk)
            {
                return Enumerable.Empty<int>();
            }

            throw new NotImplementedException();
        }

        private IEnumerable<int> ParseDayOfMonth(string dayOfMonth)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<int> ParseMonth(string month)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<int> ParseDayOfWeek(string dayOfWeek)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<int> ParseYear(string year)
        {
            throw new NotImplementedException();
        }
    }
}
