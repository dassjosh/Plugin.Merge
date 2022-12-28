using CommandLine;
using PluginMerge.Rename;

namespace PluginMerge.Commands;

[Verb("rename", HelpText = "Renames a framework class name to match the plugin class name")]
public class RenameCommand : BaseCommand<RenameCommand>
{
    [Option('f', "file", Required = true, HelpText = "Path to the framework to rename")]
    public string FileName { get; set; }
    
    [Option('n', "name", Required = true, HelpText = "Name to change the framework to")]
    public string PluginName { get; set; }

    public override async Task Execute()
    {
        await base.Execute().ConfigureAwait(false);

        RenameHandler handler = new(FileName, PluginName);
        CloseCode = await handler.Run().ConfigureAwait(false);
    }
}