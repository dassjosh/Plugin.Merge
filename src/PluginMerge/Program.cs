using CommandLine;

namespace PluginMerge;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        ParserResult<object> result = await Parser.Default.ParseArguments<InitCommand, MergeCommand>(args)
                                             .WithParsedAsync<ICommand>(c => c.Execute());
        
        //Allow all log messages to be written before exiting
        await Task.Delay(100);
        
        if (result is Parsed<object> parsed)
        {
            return ((ICommand)parsed.Value).CloseCode;
        }

        return Constants.CloseCodes.ArgsError;
    }
}