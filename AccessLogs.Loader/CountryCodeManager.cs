using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AccessLogs.Loader
{
    public class CountryCodeManager
    {
        private readonly string _requestPath;
        private SemaphoreSlim _throttler;
        private bool _doRealApiCalls;

        private ConcurrentDictionary<string, string> _codesCache = new ConcurrentDictionary<string, string>();

        public CountryCodeManager(string requestPath, int maxRequestsPerSecond, bool doRealApiCalls)
        {
            _requestPath = requestPath;
            _doRealApiCalls = doRealApiCalls;
            _throttler = new SemaphoreSlim(maxRequestsPerSecond);
        }

        public string GetCountryCode(string ipAddress)
        {
            if (!IPAddress.TryParse(ipAddress, out _))
            {
                return null;
            }

            if (_codesCache.TryGetValue(ipAddress, out string countryCode))
            {
                return countryCode;
            }

            countryCode = LoadFromApi(ipAddress);

            _codesCache.TryAdd(ipAddress, countryCode);

            return countryCode;
        }

        private string LoadFromApi(string ipAddress)
        {
            _throttler.Wait();

            try
            {
                if (!_doRealApiCalls)
                {
                    Task.Delay(1000).ContinueWith((e) => _throttler.Release());
                    return "CA";
                }

                HttpWebRequest request = WebRequest.CreateHttp($"{_requestPath}{ipAddress}");
                using WebResponse response = request.GetResponse();
                Task.Delay(1000).ContinueWith((e) => _throttler.Release());
                using Stream responseStream = response.GetResponseStream();
                using StreamReader streamReader = new StreamReader(responseStream);
                string responseJSON = streamReader.ReadToEnd();

                return JObject.Parse(responseJSON).GetValue("country_code").ToString();
            }
            catch
            {
                Task.Delay(1000).ContinueWith((e) => _throttler.Release());
                throw;
            }
        }
    }
}
