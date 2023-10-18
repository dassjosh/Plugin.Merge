using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PluginMerge.Rename;

public class RenameHandler
{
    public readonly string FileName;
    public readonly string PluginName;
    private readonly ILogger<RenameHandler> _logger;

    public RenameHandler(string fileName, string pluginName)
    {
        FileName = fileName.ToFullPath();
        PluginName = pluginName;
        _logger = LogBuilder.GetLogger<RenameHandler>();
    }
    
    public async Task<int> Run()
    {
        if (!File.Exists(FileName))
        {
            _logger.LogInformation("Failed to find file '{FileName}'", FileName);
            return Constants.CloseCodes.RenameFileNotFound;
        }

        string text = await File.ReadAllTextAsync(FileName).ConfigureAwait(false);
        
        SyntaxTree tree = CSharpSyntaxTree.ParseText(text);
        if (await tree.GetRootAsync().ConfigureAwait(false) is not CompilationUnitSyntax root)
        {
            return Constants.CloseCodes.RenameFileContainsInvalidSource;
        }

        bool changed = false;
        foreach (NamespaceDeclarationSyntax nameSpace in root.ChildNodes().OfType<NamespaceDeclarationSyntax>())
        {
            foreach (ClassDeclarationSyntax syntax in nameSpace.ChildNodes().OfType<ClassDeclarationSyntax>())
            {
                if (syntax.BaseList?.Types.Any(type => type.Type.ToString().EndsWith("Plugin")) ?? false)
                {
                    text = string.Concat(text.AsSpan(0, syntax.Identifier.SpanStart), PluginName, text.AsSpan(syntax.Identifier.SpanStart + syntax.Identifier.Span.Length));
                    changed = true;
                }
            }
        }

        if (changed)
        {
            _logger.LogInformation("Successfully renamed class name to {PluginName}", PluginName);
            await File.WriteAllTextAsync(FileName, text).ConfigureAwait(false);
        }

        return 0;
    }
}