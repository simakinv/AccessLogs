using System;
using System.IO;
using System.Linq;
using AccessLogs.Data;
using AccessLogs.Loader.LogParseStrategies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace AccessLogs.Loader
{
    class Program
    {
        public static IConfigurationRoot Configuration;

        static void Main(string[] args)
        {
            if (!args.Any() || string.IsNullOrEmpty(args[0]))
            {
                throw new Exception("File was not specified");
            }

            InitConfiguration();
            using var host = CreateHostBuilder(args).Build();

            var logProcessor = host.Services.GetService<LogFileProcessor>();
            logProcessor.ProcessFile(args[0]);
        }

        private static void InitConfiguration()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .Build();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) =>
            services
            .AddHttpClient()
            .AddTransient(provider => new AccessLogsDataManagement(Configuration.GetConnectionString("AccessLogs")))
            .AddTransient<ILogParseStrategy, ApacheLogParser>(provider =>
            {
                var parseOptions = Configuration.GetSection("ParseOptions");
                return new ApacheLogParser(parseOptions.GetValue<string>("Pattern"),
                    parseOptions.GetSection("SkipExtensions").GetChildren().Select(e => e.Value),
                    parseOptions.GetValue<string>("DateTimeFormat"));
            })
            .AddTransient<LogFileProcessor>(provider =>
            {
                return new LogFileProcessor(provider.GetService<ILogParseStrategy>(),
                    Configuration.GetValue<int>("MaxUnprocessedLogs"),
                    provider.GetService<AccessLogsDataManagement>(),
                    provider.GetService<CountryCodeManager>()
                );
            })
            .AddSingleton<CountryCodeManager>(provider =>
            {
                var countryCodeManagerSettings = Configuration.GetSection("CountryCodeManagerSettings");
                return new CountryCodeManager(countryCodeManagerSettings.GetValue<string>("RequestAddress"),
                    countryCodeManagerSettings.GetValue<int>("CallsPerSec"),
                    countryCodeManagerSettings.GetValue<bool>("DoRealApiCalls"));
            })
            );
    }
}
