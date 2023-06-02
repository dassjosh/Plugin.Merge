using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PluginMerge.Scanner;

namespace PluginMerge.Merge;

/// <summary>
/// File handler for code files
/// </summary>
public class FileHandler
{
    /// <summary>
    /// Path to the code file
    /// </summary>
    public string FilePath { get; }
        
    /// <summary>
    /// Type flags for the code inside the file
    /// </summary>
    public FileSettings Type { get; private set; }
        
    /// <summary>
    /// Data related to a plugin
    /// </summary>
    public PluginData PluginData { get; private set; }
        
    /// <summary>
    /// Basic using statements in the code file
    /// </summary>
    public List<UsingDirectiveSyntax> UsingStatements { get; } = new();

    /// <summary>
    /// Static using statements in the code file
    /// </summary>
    public List<UsingDirectiveSyntax> UsingStatics { get; } = new();

    /// <summary>
    /// Alias using statements in the code file
    /// </summary>
    public List<UsingDirectiveSyntax> UsingAliases { get; } = new();

    /// <summary>
    /// Using statements in the code file
    /// </summary>
    public List<string> DefineDirectives { get; } = new();
        
    /// <summary>
    /// Types that have been read from the file
    /// </summary>
    public List<FileType> FileTypes { get; } = new ();
        
    /// <summary>
    /// The order the file requested to be in
    /// </summary>
    public int Order { get; private set; } = 100;
    
    /// <summary>
    /// The name of the region for the file
    /// </summary>
    public string RegionName { get; }

    private readonly ILogger _logger;
    private string _text;

    /// <summary>
    /// Constructor for FileHandler.
    /// Sets the file path
    /// </summary>
    /// <param name="file"></param>
    public FileHandler(ScannedFile file)
    {
        _logger = this.GetLogger();
        FilePath = file.FileName;
        RegionName = FilePath.Replace(file.InputPath, "").TrimStart(Path.DirectorySeparatorChar);
    }

    /// <summary>
    /// Processes the code inside the file.
    /// </summary>
    /// <param name="settings">How the file should be read.</param>
    public async Task ProcessFile(PlatformSettings settings)
    {
        _logger.LogDebug("Start processing file at path: {Path}", FilePath);
        _text = await File.ReadAllTextAsync(FilePath);
        SyntaxTree tree = CSharpSyntaxTree.ParseText(_text, new CSharpParseOptions(settings.Lang));

        if (await tree.GetRootAsync() is not CompilationUnitSyntax root)
        {
            return;
        }

        await Task.WhenAll(ProcessComments(root), ProcessUsings(settings, root), ProcessNamespace(root));
    }

    private Task ProcessComments(CompilationUnitSyntax root)
    {
        _logger.LogDebug("Start processing comments file at path: {Path}", RegionName);
        
        foreach (SyntaxTrivia trivia in root.DescendantTrivia())
        {
            SyntaxKind kind = trivia.Kind();
            if (kind == SyntaxKind.SingleLineCommentTrivia)
            {
                if (trivia.Token.Parent is not (NamespaceDeclarationSyntax or ClassDeclarationSyntax or AttributeListSyntax))
                {
                    continue;
                }
                
                string commentValue = trivia.ToString();
                switch (commentValue)
                {
                    case Constants.Definitions.Framework:
                        Type |= FileSettings.Framework;
                        break;
                    case Constants.Definitions.ExcludeFile:
                        Type |= FileSettings.Exclude;
                        break;
                }

                if (commentValue.Contains(Constants.Definitions.OrderFile))
                {
                    if (int.TryParse(commentValue.Replace(Constants.Definitions.OrderFile, string.Empty), out int order))
                    {
                        Order = order;
                    }
                }

                ProcessFrameworkComments(commentValue);
            }
            else if (kind == SyntaxKind.DefineDirectiveTrivia)
            {
                DefineDirectives.Add(trivia.ToString().Replace("#define ", string.Empty));
            }
        }

        return Task.CompletedTask;
    }

    private void ProcessFrameworkComments(string comment)
    {
        Match match = Constants.Regexs.Info.Match(comment);
        if (match.Success)
        {
            PluginData ??= new PluginData();
            PluginData.SetInfo(match.Groups["Title"].Value, match.Groups["Author"].Value, match.Groups["Version"].Value);
        }

        match = Constants.Regexs.Description.Match(comment);
        if (match.Success)
        {
            PluginData ??= new PluginData();
            PluginData.SetDescription(match.Groups["Description"].Value);
        }
    }

    private Task ProcessUsings(PlatformSettings settings, CompilationUnitSyntax root)
    {
        _logger.LogDebug("Start processing usings file at path: {Path}", RegionName);
        foreach (UsingDirectiveSyntax @using in root.Usings)
        {
            if (!@using.Name.ToString().Equals(settings.Namespace))
            {
                if (@using.Alias != null)
                {
                    UsingAliases.Add(@using);
                }
                else if (@using.StaticKeyword != default)
                {
                    UsingStatics.Add(@using);
                }
                else
                {
                    UsingStatements.Add(@using);
                }
            }
        }
        
        return Task.CompletedTask;
    }
        
    private Task ProcessNamespace(CompilationUnitSyntax root)
    {
        _logger.LogDebug("Start processing namespace file at path: {Path}", RegionName);
        foreach (NamespaceDeclarationSyntax @namespace in root.ChildNodes().OfType<NamespaceDeclarationSyntax>())
        {
            foreach (BaseTypeDeclarationSyntax node in @namespace.ChildNodes().OfType<BaseTypeDeclarationSyntax>())
            {
                FileType data = new(_text, node, @namespace.Name.ToString());
                FileTypes.Add(data);
                
                if (node.BaseList != null && node.BaseList.Types.Any(type => type.Type.ToString().EndsWith("Plugin")))
                {
                    Type |= FileSettings.Plugin;
                    PluginData = new PluginData();
                    PluginData.SetPluginType(data.TypeNamespace, data.TypeName);
                }

                if (PluginData != null)
                {
                    ProcessAttributes(node);
                }
            }
        }
        
        return Task.CompletedTask;
    }

    private void ProcessAttributes(BaseTypeDeclarationSyntax node)
    {
        foreach (AttributeListSyntax list in node.AttributeLists)
        {
            foreach (AttributeSyntax attribute in list.Attributes)
            {
                if (attribute.ArgumentList == null)
                {
                    continue;
                }
                
                string name = attribute.Name.ToString();
                if (name == "Info" && attribute.ArgumentList.Arguments.Count >= 3)
                {
                    PluginData ??= new PluginData();
                    PluginData.SetInfo(attribute.ArgumentList.Arguments[0].ToString(), attribute.ArgumentList.Arguments[1].ToString(), attribute.ArgumentList.Arguments[2].ToString());
                }
                else if (name == "Description" && attribute.ArgumentList.Arguments.Count == 1)
                {
                    PluginData ??= new PluginData();
                    PluginData.SetDescription(attribute.ArgumentList.Arguments[0].ToString());
                }
            }
        }
    }
}