using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AccessLogs.Data;
using AccessLogs.Loader.LogParseStrategies;
using AccessLogs.Model;

namespace AccessLogs.Loader
{
    public class LogFileProcessor
    {
        private readonly ILogParseStrategy _logParser;
        private readonly AccessLogsDataManagement _accessLogsDataManagement;
        private readonly CountryCodeManager _countryCodeManager;
        private readonly int _maxUnprocessedLogs;

        public LogFileProcessor(ILogParseStrategy logParser,
            int maxUnprocessedLogs,
            AccessLogsDataManagement accessLogsDataManagement,
            CountryCodeManager countryCodeManager)
        {
            _logParser = logParser;
            _maxUnprocessedLogs = maxUnprocessedLogs;
            _accessLogsDataManagement = accessLogsDataManagement;
            _countryCodeManager = countryCodeManager;
        }

        public void ProcessFile(string filePath)
        {
            var logs = new List<Log>();

            Parallel.ForEach(File.ReadLines(filePath), (line) =>
            {
                var log = _logParser.Parse(line);

                if (log == null)
                {
                    return;
                }

                lock (logs)
                {
                    logs.Add(log);

                    if (logs.Count > _maxUnprocessedLogs)
                    {
                        ProcessLogs(logs);
                        logs.Clear();
                    }
                }
            });

            if (logs.Any())
            {
                ProcessLogs(logs);
            }
        }

        private void ProcessLogs(List<Log> logs)
        {
            Parallel.ForEach(logs.GroupBy(e => e.Client), (logsGroup) =>
            {
                var countryCode = _countryCodeManager.GetCountryCode(logsGroup.Key);

                foreach (var log in logsGroup)
                {
                    log.CountryCode = countryCode;
                }
            });

            _accessLogsDataManagement.SaveLogs(logs);
        }
    }
}