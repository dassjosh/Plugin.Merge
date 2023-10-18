using System.Collections.Concurrent;

namespace PluginMerge.Logging;

public static class LogBuilder
{
    private static readonly ConcurrentDictionary<Type, ILogger> Loggers = new();
    private static ILoggerFactory _factory;

    public static void InitLogger(LogLevel logLevel)
    {
        _factory = LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddFilter("PluginMerge", logLevel)
                .AddConsoleFormatter<LogFormatter, LogFormatterOptions>()
                .AddConsole(options =>
                {
                    options.FormatterName = "PluginMergeFormatter";
                });
        });
    }

    public static ILogger<T> GetLogger<T>()
    {
        if (Loggers.TryGetValue(typeof(T), out ILogger cachedLogger))
        {
            return (ILogger<T>)cachedLogger;
        }

        ILogger<T> logger = _factory.CreateLogger<T>();
        Loggers[typeof(T)] = logger;
        return logger;
    }
}