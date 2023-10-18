using CommandLine;

namespace PluginMerge;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        ParserResult<object> result = await Parser.Default.ParseArguments<InitCommand, MergeCommand, RenameCommand>(args)
                                                  .WithParsedAsync<ICommand>(c => c.Execute()).ConfigureAwait(false);

        if (result is Parsed<object> parsed)
        {
            return ((ICommand)parsed.Value).CloseCode;
        }

        return Constants.CloseCodes.ArgsError;
    }
}