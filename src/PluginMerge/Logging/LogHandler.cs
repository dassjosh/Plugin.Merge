using System.Collections.Concurrent;

namespace PluginMerge.Logging;

public static class LogHandler
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

    public static ILogger GetLogger(this object entity)
    {
        if (Loggers.TryGetValue(entity.GetType(), out ILogger logger))
        {
            return logger;
        }

        logger = _factory.CreateLogger(entity.GetType());
        Loggers[entity.GetType()] = logger;
        return logger;
    }
}