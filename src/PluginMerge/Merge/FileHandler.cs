using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
    private FileSettings Settings { get; set; }
        
    /// <summary>
    /// Data related to a plugin
    /// </summary>
    public PluginData PluginData { get; private set; }
        
    /// <summary>
    /// Using statements in the code file
    /// </summary>
    public List<UsingDirectiveSyntax> UsingStatements { get; } = new();
    
    /// <summary>
    /// //References: comments
    /// </summary>
    public List<string> References { get; } = new();
    
    /// <summary>
    /// //Requires: comments
    /// </summary>
    public List<string> Requires { get; } = new();
    
    /// <summary>
    /// Using statements in the code file
    /// </summary>
    public List<string> DefineDirectives { get; } = new();
        
    /// <summary>
    /// Types that have been read from the file
    /// </summary>
    public List<IFileType> FileTypes { get; } = new ();
        
    /// <summary>
    /// The order the file requested to be in
    /// </summary>
    public int Order { get; private set; } = 1000;
    
    /// <summary>
    /// The name of the region for the file
    /// </summary>
    public string RegionName { get; }

    private readonly ILogger _logger;
    private string _sourceCode;

    /// <summary>
    /// Constructor for FileHandler.
    /// Sets the file path
    /// </summary>
    /// <param name="file"></param>
    public FileHandler(ScannedFile file)
    {
        _logger = LogBuilder.GetLogger<FileHandler>();
        FilePath = file.FileName;
        RegionName = FilePath.Replace(file.InputPath, "").TrimStart(Path.DirectorySeparatorChar);
    }

    /// <summary>
    /// Processes the code inside the file.
    /// </summary>
    /// <param name="settings">How the file should be read.</param>
    /// <param name="options">Parsing options</param>
    public async Task ProcessFile(PlatformSettings settings, CSharpParseOptions options)
    {
        _logger.LogDebug("Start processing file at path: {Path}", FilePath);
        _sourceCode = await File.ReadAllTextAsync(FilePath).ConfigureAwait(false);
        SyntaxTree tree = CSharpSyntaxTree.ParseText(_sourceCode, options);

        if (await tree.GetRootAsync().ConfigureAwait(false) is not CompilationUnitSyntax root)
        {
            return;
        }

        await ProcessComments(root).ConfigureAwait(false);
        if (IsExcludedFile())
        {
            return;
        }

        await Task.WhenAll(ProcessUsings(settings, root), ProcessNamespace(settings, root)).ConfigureAwait(false);
    }

    private Task ProcessComments(CompilationUnitSyntax root)
    {
        _logger.LogDebug("Start processing comments file at path: {Path}", RegionName);
        
        foreach (SyntaxTrivia trivia in root.DescendantTrivia())
        {
            if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                if (trivia.Token.Parent is not (BaseNamespaceDeclarationSyntax or ClassDeclarationSyntax or AttributeListSyntax or UsingDirectiveSyntax))
                {
                    continue;
                }

                string comment = trivia.ToString();
                if (comment == Constants.Definitions.Framework)
                {
                    Settings |= FileSettings.Framework;
                } 
                else if (comment == Constants.Definitions.ExcludeFile)
                {
                    Settings |= FileSettings.Exclude;
                    return Task.CompletedTask;
                } 
                else if (comment.Contains(Constants.Definitions.OrderFile) && int.TryParse(comment.Replace(Constants.Definitions.OrderFile, string.Empty), out int order))
                {
                    Order = order;
                }
                else if (comment.StartsWith(Constants.OxideDefinitions.Reference, StringComparison.OrdinalIgnoreCase))
                {
                    References.Add(comment[Constants.OxideDefinitions.Reference.Length..]);
                }
                else if (comment.StartsWith(Constants.OxideDefinitions.Requires, StringComparison.OrdinalIgnoreCase))
                {
                    Requires.Add(comment[Constants.OxideDefinitions.Requires.Length..]);
                }

                ProcessFrameworkComments(comment);
            }
            else if (trivia.IsKind(SyntaxKind.DefineDirectiveTrivia))
            {
                const string define = "#define ";
                DefineDirectives.Add(trivia.ToString().Substring(define.Length));
            }
        }

        return Task.CompletedTask;
    }

    private void ProcessFrameworkComments(string comment)
    {
        if (comment.StartsWith("//[Info"))
        {
            Match match = Constants.Regexs.Info.Match(comment);
            if (match.Success)
            {
                PluginData ??= new PluginData();
                PluginData.SetInfo(match.Groups["Title"].Value, match.Groups["Author"].Value, match.Groups["Version"].Value);
            }
        }
        else if (comment.StartsWith("//[Description("))
        {
            Match match = Constants.Regexs.Description.Match(comment);
            if (match.Success)
            {
                PluginData ??= new PluginData();
                PluginData.SetDescription(match.Groups["Description"].Value);
            }
        }
    }

    private Task ProcessUsings(PlatformSettings settings, CompilationUnitSyntax root)
    {
        _logger.LogDebug("Start processing usings file at path: {Path}", RegionName);
        AddUsings(settings, root.Usings);
        return Task.CompletedTask;
    }
    
    private void AddUsings(PlatformSettings settings, SyntaxList<UsingDirectiveSyntax> usings)
    {
        foreach (UsingDirectiveSyntax @using in usings)
        {
            string name = @using.Name.ToString();
            if (!name.Equals(settings.Namespace))
            {
                UsingStatements.Add(@using);
            }
        }
    }

    private Task ProcessNamespace(PlatformSettings settings, CompilationUnitSyntax root)
    {
        _logger.LogDebug("Start processing namespace file at path: {Path}", RegionName);
        foreach (BaseNamespaceDeclarationSyntax @namespace in root.ChildNodes().OfType<BaseNamespaceDeclarationSyntax>())
        {
            AddUsings(settings, @namespace.Usings);
            
            foreach (BaseTypeDeclarationSyntax node in @namespace.ChildNodes().OfType<BaseTypeDeclarationSyntax>())
            {
                FileSettings typeSettings = Settings;
                foreach (SyntaxTrivia trivia in node.DescendantTrivia())
                {
                    if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
                    {
                        if (trivia.ToString() == Constants.Definitions.ExtensionFile)
                        {
                            typeSettings |= FileSettings.Extension;
                        }
                    }
                }
                
                FileType data = new(_sourceCode, node, @namespace.Name.ToString(), typeSettings);
                FileTypes.Add(data);
                
                if (!IsFrameworkFile() && node.BaseList is not null && node.BaseList.Types.Any(type => type.Type.ToString().EndsWith("Plugin")))
                {
                    Settings |= FileSettings.Plugin;
                    PluginData = new PluginData();
                    PluginData.SetPluginType(data.TypeNamespace, data.TypeName);
                    PluginData.SetBaseTypes(node.BaseList.Types);
                }

                if (PluginData is not null)
                {
                    ProcessAttributes(node);
                }
            }

            foreach (DelegateDeclarationSyntax @delegate in @namespace.ChildNodes().OfType<DelegateDeclarationSyntax>())
            {
                DelegateType data = new(_sourceCode, @delegate, @namespace.Name.ToString());
                FileTypes.Add(data);
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
                if (attribute.ArgumentList is null)
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

    public bool IsPluginFile()
    {
        return Settings.HasFlag(FileSettings.Plugin);
    }

    public bool IsFrameworkFile()
    {
        return Settings.HasFlag(FileSettings.Framework);
    }
    
    public bool IsExcludedFile()
    {
        return Settings.HasFlag(FileSettings.Exclude);
    }
}