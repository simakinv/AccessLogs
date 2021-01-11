using AccessLogs.Model;

namespace AccessLogs.Loader.LogParseStrategies
{
    public interface ILogParseStrategy
    {
        Log Parse(string line);
    }
}
