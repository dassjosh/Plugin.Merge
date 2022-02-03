using CommandLine;
using CommandLine.Text;

namespace PluginMerge.Commands;

[Verb("init", HelpText = "Creates a new merge.json configuration in the current directory")]
public class InitCommand : BaseCommand
{
    [Option('p', "path", Required = false, HelpText = "Path to create the merge.json configuration file in", Default = "./")]
    public string FilePath { get; set; } = "./";

    [Option('f', "filename", Required = false, HelpText = "Name of the outputted configuration file", Default = "merge.yml")]
    public string FileName { get; set; } = "merge.yml";

    public override async Task Execute()
    {
        await base.Execute();
        
        try
        {
            string path = Path.Combine(FilePath, FileName).ToFullPath();
            await PluginMergeConfigHandler.Instance.Create(path);
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex, "An error occured creating the config file");
            CloseCode = Constants.CloseCodes.InitError;
        }
    }

    [Usage(ApplicationAlias = "plugin.merge")]
    public static IEnumerable<Example> Examples => new List<Example>
    {
        new("Create config in current directory",new UnParserSettings{PreferShortName = true}, new InitCommand { FileName = "merge.json" }),
        new("Create config in specified directory", new UnParserSettings{PreferShortName = true}, new InitCommand { FilePath = @"C:\Users\USERNAME\Source\Repos\MyFirstMergePlugin", FileName = "merge.json" })
    };
}