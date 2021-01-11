using System;

namespace AccessLogs.Model
{
    public class Log
    {
        public DateTimeOffset Date { get; set; }
        public string Client { get; set; }
        public string Path { get; set; }
        public string QueryParameters { get; set; }
        public int StatusCode { get; set; }
        public int Size { get; set; }
        public string CountryCode { get; set; }
    }
}
