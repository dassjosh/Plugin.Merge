namespace PluginMerge.Commands;

public interface ICommand
{
    Task Execute();
    
    int CloseCode { get; }
}