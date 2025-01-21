using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PluginMerge.Merge;

public class DelegateType : IFileType
{
    public string TypeName { get; }
    public string TypeNamespace { get; }
    
    private readonly string _sourceCode;
    private readonly DelegateDeclarationSyntax _type;

    public DelegateType(string source, DelegateDeclarationSyntax type, string typeNamespace)
    {
        _sourceCode = source;
        _type = type;
        TypeName = type.Identifier.ToString();
        TypeNamespace = typeNamespace;
    }
    
    public void Write(CodeWriter writer)
    {
        foreach (ReadOnlySpan<char> line in GetCode().EnumerateLines())
        {
            writer.WriteCode(line.Trim());
        }
    }

    private ReadOnlySpan<char> GetCode()
    {
        int startIndex = _type.Span.Start;
        int endIndex = _type.Span.End;
        
        return _sourceCode.AsSpan().Slice(startIndex, endIndex - startIndex).Trim();
    }

    public bool HasCode()
    {
        return !GetCode().IsEmpty;
    }

    public void AddSettings(FileSettings settings)
    {
        throw new NotImplementedException();
    }

    public bool IsPlugin() => false;

    public bool IsExtensionMethods() => false;
}