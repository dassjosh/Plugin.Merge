using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PluginMerge.Merge;

/// <summary>
/// Data about the plugin to be written in the final output
/// </summary>
public class PluginData
{
    /// <summary>
    /// Plugin Title
    /// </summary>
    public string Title { get; private set; }
        
    /// <summary>
    /// Plugin Version
    /// </summary>
    public string Version { get; private set; }
        
    /// <summary>
    /// Plugin Author
    /// </summary>
    public string Author { get; private set; }

    /// <summary>
    /// Plugin Description
    /// </summary>
    public string Description { get; private set; }
    
    /// <summary>
    /// Plugin Namespace
    /// </summary>
    public string NameSpace { get; private set; }
    
    /// <summary>
    /// Plugin Class Name
    /// </summary>
    public string ClassName { get; private set; }

    /// <summary>
    /// Interfaces On the Plugin
    /// </summary>
    public List<string> PluginBaseTypes { get; } = new();

    public void SetInfo(string title, string author, string version)
    {
        Title = title;
        Author = author;
        Version = version;
    }

    public void SetDescription(string description)
    {
        Description = description;
    }

    public void SetPluginType(string @namespace, string className)
    {
        NameSpace = @namespace;
        ClassName = className;
    }

    public void SetBaseTypes(SeparatedSyntaxList<BaseTypeSyntax> types)
    {
        foreach (BaseTypeSyntax typeSyntax in types)
        {
            string type = typeSyntax.ToString();
            PluginBaseTypes.Add(type);
        }
    }
}