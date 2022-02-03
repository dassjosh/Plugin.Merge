using CommandLine;
using CommandLine.Text;

namespace PluginMerge.Commands;

[Verb("merge", true, HelpText = "Merges multiple .cs files into a single plugin/framework file.")]
public class MergeCommand : BaseCommand
{
    [Option('p', "path", Required = false, HelpText = "Path to the merge.json configuration file", Default = "./merge.yml")]
    public string ConfigPath { get; set; } = "./merge.yml";
    
    [Option('m', "merge", Group = "Mode", HelpText = "Enables merge mode to merge files into a single plugin/framework file", Default = false)]
    public bool Merge { get; set; }
    
    [Option('c', "compile", Group = "Mode", HelpText = "Enables compile mode. Will attempt to compile merged file and display any errors if it fails.", Default = false)]
    public bool Compile { get; set; }

    [Option('o', "output", HelpText = "Additional output paths for the generated code file")]
    public IEnumerable<string> OutputPaths { get; set; }

    public override async Task Execute()
    {
        await base.Execute();
        
        PluginMergeConfig config;
        
        try
        {
            string configFile = ConfigPath.ToFullPath();
            Logger.LogInformation("Loading Plugin Merge Config At {File}", configFile);
            config = await PluginMergeConfigHandler.Instance.Load(configFile);
            if (config == null)
            {
                CloseCode = Constants.CloseCodes.MergeConfigNotFoundError;
                return;
            }

            string path = Path.GetDirectoryName(configFile);
            if (!string.IsNullOrEmpty(path))
            {
                Directory.SetCurrentDirectory(path);
            }

            if (OutputPaths != null)
            {
                config.Merge.OutputPaths.AddRange(OutputPaths);
            }
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex, "An error occured loading the config file");
            CloseCode = Constants.CloseCodes.MergeConfigError;
            return;
        }

        if (Merge)
        {
            try
            {
                MergeHandler handler = new(config);
                await handler.Run();
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, "An error occured merging files");
                CloseCode = Constants.CloseCodes.MergeFilesError;
                return;
            }
        }

        if (Compile)
        {
            try
            {
                CompileHandler compile = new(config.Merge.FinalFiles.FirstOrDefault(), config);
                await compile.Run();
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, "An error compiling merged file");
                CloseCode = Constants.CloseCodes.CompileFilesError;
            }
        }
    }
    
    [Usage(ApplicationAlias = "plugin.merge")]
    public static IEnumerable<Example> Examples => new List<Example>
    {
        new("Merge and Compile", new UnParserSettings{PreferShortName = true}, new MergeCommand { ConfigPath = "./merge.json", Compile = true, Merge = true}),
        new("Merge Only", new UnParserSettings{PreferShortName = true}, new MergeCommand { ConfigPath = @"C:\Users\USERNAME\Source\Repos\MyFirstMergePlugin\merge.json", Merge = true}),
        new("Merge Additional Output Paths", new UnParserSettings{PreferShortName = true}, new MergeCommand { ConfigPath = "./merge.json", Merge = true, Compile = true, OutputPaths = new List<string>{"./additional/output/path"}}),
        new("Compile Only", new UnParserSettings{PreferShortName = true}, new MergeCommand { ConfigPath = "./merge.json", Compile = true})
    };
}