using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PluginMerge.Merge;

/// <summary>
/// Represents an object type
/// </summary>
public class FileType : IFileType
{
    public string TypeName { get; }
    public string TypeNamespace { get; }
    private FileSettings _settings;

    private readonly string _sourceCode;
    private readonly BaseTypeDeclarationSyntax _type;

    /// <summary>
    /// Constructor for enum file type
    /// </summary>
    /// <param name="source"></param>
    /// <param name="type"></param>
    /// <param name="typeNamespace"></param>
    /// <param name="settings">Settings for the type</param>
    public FileType(string source, BaseTypeDeclarationSyntax type, string typeNamespace, FileSettings settings)
    {
        _sourceCode = source;
        _type = type;
        TypeName = type.Identifier.ToString();
        TypeNamespace = typeNamespace;
        _settings = settings;
    }

    /// <summary>
    /// Writes the code lines to the writer
    /// </summary>
    /// <param name="writer"></param>
    public void Write(CodeWriter writer)
    {
        foreach (ReadOnlySpan<char> line in GetCode().EnumerateLines())
        {
            writer.WriteCode(line.Trim());
        }
    }
        
    /// <summary>
    /// Returns the code for the given type
    /// </summary>
    /// <returns></returns>
    private ReadOnlySpan<char> GetCode()
    {
        int startIndex = _type.Span.Start;
        int endIndex = _type.Span.End;
        
        //If plugin type we want code between the open and close braces
        if (IsPlugin())
        {
            startIndex = _type.OpenBraceToken.SpanStart + 1;
            endIndex = _type.CloseBraceToken.SpanStart - 1;
        }
        
        return _sourceCode.AsSpan().Slice(startIndex, endIndex - startIndex).Trim();
    }

    public bool HasCode()
    {
        return !GetCode().IsEmpty;
    }

    public bool ContainsType(string type)
    {
        return _sourceCode.AsSpan().Slice(_type.SpanStart, _type.Span.Length).Contains(type.AsSpan(), StringComparison.CurrentCulture);
    }

    public void AddSettings(FileSettings settings)
    {
        _settings |= settings;
    }

    public bool IsPlugin()
    {
        return _settings.HasFlag(FileSettings.Plugin);
    }
    
    public bool IsFramework()
    {
        return _settings.HasFlag(FileSettings.Framework);
    }
    
    public bool IsExtensionMethods()
    {
        return _settings.HasFlag(FileSettings.Extension);
    }
}