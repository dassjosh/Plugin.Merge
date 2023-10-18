using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace PluginMerge.Compile;

public class CompileHandler
{
    private readonly ILogger _logger;
    private readonly string _fileName;
    private readonly PluginMergeConfig _config;
    private readonly CompileConfig _compile;

    public CompileHandler(string fileName, PluginMergeConfig config)
    {
        _logger = LogBuilder.GetLogger<CompileHandler>();
        _fileName = fileName;
        _config = config;
        _compile = config.Compile;
    }

    public async Task Run()
    {
        Stopwatch sw = Stopwatch.StartNew();
        _logger.LogInformation("Starting Merged File Compilation Version: {Version}", typeof(Program).Assembly.GetName().Version);
        
        if (!File.Exists(_fileName))
        {
            _logger.LogError("File does not exist at path: {Path}", _fileName);
            return;
        }

        string code = await File.ReadAllTextAsync(_fileName).ConfigureAwait(false);
        SyntaxTree tree = CSharpSyntaxTree.ParseText(code, new CSharpParseOptions(_config.PlatformSettings.Lang));

        FileScanner scanner = new(_compile.AssemblyPaths, "*.dll", _compile.IgnorePaths, _compile.IgnoreFiles);

        List<MetadataReference> references = new();
        foreach (ScannedFile file in scanner.ScanFiles())
        {
            _logger.LogDebug("Added Assembly Reference: {File}", file.FileName);
            references.Add(MetadataReference.CreateFromFile(file.FileName));
        }

        await using MemoryStream stream = new();
        
        CSharpCompilation compilation = CSharpCompilation.Create("output.dll", new List<SyntaxTree> {tree}, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        EmitResult results = compilation.Emit(stream);
        foreach (Diagnostic diagnostic in results.Diagnostics.Where(r => r.Severity >= _compile.CompileLogLevel).OrderBy(r => (int)r.Severity))
        {
            switch (diagnostic.Severity)
            {
                case DiagnosticSeverity.Error:
                    _logger.LogError("{Message}", diagnostic.ToString());
                    break;
                case DiagnosticSeverity.Warning:
                    _logger.LogWarning("{Message}", diagnostic.ToString());
                    break;
                case DiagnosticSeverity.Info:
                case DiagnosticSeverity.Hidden:
                    _logger.LogInformation("{Message}", diagnostic.ToString());
                    break;
            }
        }
        
        sw.Stop();
        _logger.LogInformation("Merged File Compiled in {Time}ms", sw.ElapsedMilliseconds);
        
        if (!results.Success)
        {
            _logger.LogError("Merge File failed to compile");
            return;
        }
        
        _logger.LogInformation("Merge File Compiled Successfully");
    }
}