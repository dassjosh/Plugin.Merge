using CommandLine;

namespace PluginMerge.Commands;

public class BaseCommand : ICommand
{
    [Option('d', "debug", Required = false, HelpText = "Enable debug log output")]
    public bool Debug { get; set; }
    
    public int CloseCode { get; protected set; } = Constants.CloseCodes.NoError;
    
    protected ILogger Logger;
    
    public virtual Task Execute()
    {
        LogHandler.InitLogger(Debug ? LogLevel.Debug : LogLevel.Information);
        Logger = this.GetLogger();
        return Task.CompletedTask;
    }
}