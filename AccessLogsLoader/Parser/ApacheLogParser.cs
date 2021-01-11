using AccessLogs.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace AccessLogs.Loader.LogParseStrategies
{
    public class ApacheLogParser: ILogParseStrategy
    {
        private readonly Regex _regex;
        private readonly IEnumerable<string> _skipExtensions;
        private readonly string _dateTimeFormat;

        public ApacheLogParser(string pattern, IEnumerable<string> skipExtensions, string dateTimeFormat)
        {
            _regex = new Regex(pattern, RegexOptions.ExplicitCapture | RegexOptions.Compiled);
            _skipExtensions = skipExtensions;
            _dateTimeFormat = dateTimeFormat;
        }

        public Log Parse(string line)
        {
            var matches = _regex.Match(line).Groups;

            if (matches.Count <= 1)
            {
                return null;
            }

            var request = matches["request"].Value;
            var paramsStartIndex = request.IndexOf("?", StringComparison.Ordinal);
            var path = paramsStartIndex == -1 ? request : request.Substring(0, paramsStartIndex);

            if (_skipExtensions.Any(a => path.EndsWith(a, StringComparison.OrdinalIgnoreCase)))
            {
                return null;
            }

            var log = new Log();
            log.Client = matches["client"].Value;
            log.Date = DateTimeOffset.ParseExact(matches["date"].Value, _dateTimeFormat, new DateTimeFormatInfo());
            log.Path = path;
            log.QueryParameters = paramsStartIndex == -1 ? null : request.Substring(paramsStartIndex + 1);
            log.StatusCode = Int32.Parse(matches["code"].Value);

            if (int.TryParse(matches["size"].Value, out int size))
            {
                log.Size = size;
            }

            return log;
        }
    }
}
