//Sourced from https://docs.microsoft.com/en-us/dotnet/core/extensions/console-log-formatter

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace PluginMerge.Logging;

public class LogFormatter : ConsoleFormatter, IDisposable
{
    private readonly IDisposable _optionsReloadToken;
    private LogFormatterOptions FormatterOptions { get; set; }

    public LogFormatter(IOptionsMonitor<LogFormatterOptions> options)
        : base("PluginMergeFormatter")
    {
        ReloadLoggerOptions(options.CurrentValue);
        _optionsReloadToken = options.OnChange(ReloadLoggerOptions);
    }

    private void ReloadLoggerOptions(LogFormatterOptions options)
    {
        FormatterOptions = options;
    }

    public void Dispose()
    {
        _optionsReloadToken?.Dispose();
    }

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider scopeProvider, TextWriter textWriter)
    {
        LogLevel logLevel = logEntry.LogLevel;
        ConsoleColors logLevelColors = GetLogLevelConsoleColors(logLevel);

        textWriter.WriteStartColor(logLevelColors.Background, logLevelColors.Foreground);
        textWriter.Write(GetLogLevelString(logLevel));
        textWriter.Write(' ');

        if (logEntry.Category.StartsWith("PluginMerge"))
        {
            int startIndex = logEntry.Category.LastIndexOf(".", StringComparison.Ordinal) + 1;
            textWriter.Write(logEntry.Category.AsSpan(startIndex, logEntry.Category.Length - startIndex));
        }
        else
        {
            textWriter.Write(logEntry.Category);
        }

        textWriter.Write(':');
        
        string message = logEntry.Formatter!(logEntry.State, logEntry.Exception);
        WriteMessage(textWriter, message);
        
        Exception exception = logEntry.Exception;
        if (exception != null)
        {
            WriteMessage(textWriter, exception.ToString());
        }

        textWriter.Write(Environment.NewLine);
        textWriter.WriteEndColor();
    }
    
    private void WriteMessage(TextWriter textWriter, string message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            textWriter.Write(' ');
            textWriter.Write(message);
        }
    }

    private static string GetLogLevelString(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "[trace]",
            LogLevel.Debug => "[debug]",
            LogLevel.Information => "[info] ",
            LogLevel.Warning => "[warn] ",
            LogLevel.Error => "[fail] ",
            LogLevel.Critical => "[crit] ",
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
        };
    }

    private ConsoleColors GetLogLevelConsoleColors(LogLevel logLevel)
    {
        // We must explicitly set the background color if we are setting the foreground color,
        // since just setting one can look bad on the users console.
        return logLevel switch
        {
            LogLevel.Trace => new ConsoleColors(ConsoleColor.Gray, ConsoleColor.Black),
            LogLevel.Debug => new ConsoleColors(ConsoleColor.Gray, ConsoleColor.Black),
            LogLevel.Information => new ConsoleColors(ConsoleColor.DarkGreen, ConsoleColor.Black),
            LogLevel.Warning => new ConsoleColors(ConsoleColor.DarkYellow, ConsoleColor.Black),
            LogLevel.Error => new ConsoleColors(ConsoleColor.DarkRed, ConsoleColor.Black),
            LogLevel.Critical => new ConsoleColors(ConsoleColor.DarkRed, ConsoleColor.Black),
            _ => new ConsoleColors(null, null)
        };
    }

    private readonly struct ConsoleColors
    {
        public ConsoleColors(ConsoleColor? foreground, ConsoleColor? background)
        {
            Foreground = foreground;
            Background = background;
        }

        public ConsoleColor? Foreground { get; }

        public ConsoleColor? Background { get; }
    }
}