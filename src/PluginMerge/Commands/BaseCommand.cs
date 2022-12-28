using CommandLine;

namespace PluginMerge.Commands;

public abstract class BaseCommand<T> : ICommand
{
    [Option('d', "debug", Required = false, HelpText = "Enable debug log output", Hidden = true)]
    public bool Debug { get; set; }
    
    [Option('l', "log", Required = false, HelpText = "Log Level for output")]
    public LogLevel? LoggerLevel { get; set; }
    
    public int CloseCode { get; protected set; } = Constants.CloseCodes.NoError;
    
    protected ILogger Logger { get; private set; }
    
    public virtual Task Execute()
    {
        LogLevel level = LogLevel.Information;
        if (Debug)
        {
            level = LogLevel.Debug;
        }

        if (LoggerLevel.HasValue)
        {
            level = LoggerLevel.Value;
        }
        
        LogBuilder.InitLogger(level);
        Logger = LogBuilder.GetLogger<T>();
        return Task.CompletedTask;
    }
}