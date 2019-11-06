using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Rinsen.CronScheduler
{
    public class CronParser
    {
        public const string Asterisk = "*";
        private readonly int[] ValidMinuteSlashes = new[] { 2, 3, 4, 5, 6, 10, 12, 15, 20, 30 };
        private readonly int[] ValidHourSlashes = { 2, 3, 4, 6, 8, 12 };

        private readonly ICronDateTimeService _cronDateTimeService;
        private readonly IReadOnlyCollection<int> _emptyCollection = new ReadOnlyCollection<int>(new int[0]);

        public CronParser(ICronDateTimeService cronDateTimeService)
        {
            _cronDateTimeService = cronDateTimeService;
        }

        /// <summary>
        /// minute (0 - 59) hour (0 - 23) day of the month (1 - 31) month (1 - 12) day of the week (0 - 6) (Sunday 0 to Saturday 6) Year 
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public CronExpression Parse(string expression)
        {
            if (string.IsNullOrEmpty(expression))
            {
                throw new ArgumentException("Empty expression is not valid", nameof(expression));
            }

            if (expression.Contains("?") || expression.Contains("L") || expression.Contains("W") || expression.Contains("#"))
            {
                throw new ArgumentOutOfRangeException(nameof(expression), expression, "?, L, W and # is not supported in this implementation");
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
            if (parts.Length == 6)
            {
                year = parts[5];
            }

            var cronExpression = new CronExpression(_cronDateTimeService,
                ParseString(minutes, nameof(minutes), 60, ValidMinuteSlashes),
                ParseString(hours, nameof(hours), 24 , ValidHourSlashes),
                ParseString(dayOfMonth, nameof(dayOfMonth), 31),
                ParseString(month, nameof(month), 12),
                ParseString(dayOfWeek, nameof(dayOfWeek), 6),
                ParseString(year, nameof(year), 9999));

            return cronExpression;
        }

        private IReadOnlyCollection<int> ParseString(string data, string parameterName, int maxValue, int[] validSlashValues = null)
        {
            if (data == Asterisk)
            {
                return _emptyCollection;
            }

            if (data.Contains('-'))
            {
                var dataRange = data.Split('-');
                var start = int.Parse(dataRange[0]);
                var end = int.Parse(dataRange[1]);

                return Enumerable.Range(start, end - start + 1).ToArray();
            }

            if (data.Contains('/') && validSlashValues != null)
            {
                var dataSlash = data.Split('/');

                if (!dataSlash[0].Equals(Asterisk))
                    throw new ArgumentOutOfRangeException(parameterName, dataSlash[0], "First part in Slash expression must be Asterisk");

                var range = int.Parse(dataSlash[1]);

                if (!validSlashValues.Contains(range))
                    throw new ArgumentOutOfRangeException(parameterName, range, $"Only {string.Join(",", validSlashValues)} is valid");

                var slashRangeResult = new List<int>();
                var iterator = 0;
                while (iterator < maxValue)
                {
                    slashRangeResult.Add(iterator);
                    iterator += range;
                }
                return slashRangeResult;
            }

            var dataParts = data.Split(',');

            var result = new List<int>(dataParts.Length);
            foreach (var dataItem in dataParts)
            {
                result.Add(int.Parse(dataItem));
            }

            return result;
        }
    }
}
