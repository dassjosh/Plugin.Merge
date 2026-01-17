using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PluginMerge.Creator;

/// <summary>
/// 
/// </summary>
public class FileCreator
{
    private readonly List<FileHandler> _files;
    private readonly PluginMergeConfig _settings;

    private FileHandler _plugin;
    /// <summary>
    /// Files that are part of the plugin
    /// </summary>
    private readonly List<FileHandler> _pluginFiles = new();
        
    /// <summary>
    /// Files that contain data types
    /// </summary>
    private readonly List<FileHandler> _dataFiles = new();
    
    /// <summary>
    /// Files that contain extension methods
    /// </summary>
    private readonly List<IFileType> _extensionTypes = new();
        
    /// <summary>
    /// Files that contain frameworks
    /// </summary>
    private readonly List<FileHandler> _frameworks = new();

    private readonly bool IsPluginMode;
    private readonly bool IsFrameworkMode;
    private readonly bool IsMergeFrameworkMode;

    /// <summary>
    /// Code writer for the final file
    /// </summary>
    private CodeWriter _writer;

    private readonly ILogger _logger;

    /// <summary>
    /// Constructor for the base creator
    /// </summary>
    /// <param name="settings">Creator settings</param>
    /// <param name="files">Files that were processed</param>
    public FileCreator(PluginMergeConfig settings, List<FileHandler> files)
    {
        _logger = LogBuilder.GetLogger<FileCreator>();
        _files = files;
        _settings = settings;
        IsPluginMode = settings.Merge.CreatorMode is CreatorMode.Plugin;
        IsFrameworkMode = settings.Merge.CreatorMode is CreatorMode.Framework;
        IsMergeFrameworkMode = settings.Merge.CreatorMode is CreatorMode.MergeFramework;
    }

    /// <summary>
    /// Creates the CodeWriter for the plugin
    /// </summary>
    public bool Create()
    {
        _plugin = FindPlugin();
        if (_plugin?.PluginData is null)
        {
            _logger.LogError("No Plugin Found in Files");
            return false;
        }

        FilterFiles(_plugin.PluginData);
        
        _writer = new CodeWriter(_plugin.PluginData, _settings.Merge);
        bool hasExtensionMethods = _extensionTypes.Count != 0;

        WriteRequires();
        WriteReferences();
        WriteRequiredPreprocessorDirectives();
        WriteDefines();
        WriteUsings();
        WriteNamespace(!hasExtensionMethods);
        WriteNamespaceUsings();
        if (IsMergeFrameworkMode) _writer.WriteFramework();
        StartPluginClass();
        if (IsPluginMode || IsFrameworkMode)
        {
            WritePluginAndDataFiles();
        }
        else
        {
            WritePluginFiles();
        }
        EndPluginClass();
        if (IsMergeFrameworkMode) WriteDataFiles();
        WriteFrameworks();
        if (hasExtensionMethods)
        {
            EndNamespace();
            WriteExtensionMethods();
        }
        WriteErrorPreprocessorMessages();
        return true;
    }

    /// <summary>
    /// Returns the file that has plugin declarations
    /// </summary>
    /// <returns></returns>
    private FileHandler FindPlugin()
    {
        return _files.FirstOrDefault(f => f.IsPluginFile());
    }

    /// <summary>
    /// Filters the files into their given types
    /// </summary>
    private void FilterFiles(PluginData data)
    {
        foreach (FileHandler file in _files)
        {
            bool isPluginType = false;
            foreach (IFileType type in file.FileTypes)
            {
                if (type.IsExtensionMethods())
                {
                    _extensionTypes.Add(type);
                    continue;
                }
                
                if (type.TypeNamespace == data.NameSpace && type.TypeName == data.ClassName)
                {
                    type.AddSettings(FileSettings.Plugin);
                    isPluginType = true;
                }
            }
            
            if (file.IsFrameworkFile())
            {
                _frameworks.Add(file);
            } 
            else if (isPluginType)
            {
                _pluginFiles.Add(file);
            }
            else
            {
                _dataFiles.Add(file);
            }
        }
    }

    private void WriteRequiredPreprocessorDirectives()
    {
        PreprocessorDirectives settings = _settings.PreprocessorDirectives;
        if (settings.HasDirectives)
        {
            _writer.WriteRequiredPreprocessorDirectives(settings.EnabledDirectives);
        }
    }

    private void WriteErrorPreprocessorMessages()
    {
        PreprocessorDirectives settings = _settings.PreprocessorDirectives;
        if (settings.HasDirectives)
        {
            _writer.WritePreprocessorDirectiveError(settings.EnabledDirectives);
            _writer.WriteEndPreprocessorDirectives();
        }
    }
    
    /// <summary>
    /// writes oxide //References: to the code writer
    /// </summary>
    private void WriteReferences()
    {
        _writer.WriteReferences(_files.SelectMany(f => f.References).Concat(_settings.Merge.References).Distinct());
    }
    
    /// <summary>
    /// writes oxide //Requires: to the code writer
    /// </summary>
    private void WriteRequires()
    {
        _writer.WriteRequires(_files.SelectMany(f => f.Requires).Concat(_settings.Merge.Requires).Distinct());
    }
    
    /// <summary>
    /// writes usings to the code writer
    /// </summary>
    private void WriteDefines()
    {
        _writer.WriteDefines(_files
                            .SelectMany(f => f.DefineDirectives)
                            .Concat(_settings.Merge.Defines)
                            .Distinct());
    }

    /// <summary>
    /// writes usings to the code writer
    /// </summary>
    private void WriteUsings()
    {
        string[] extensionNameSpaces = _files.SelectMany(f => f.FileTypes
                                                             .Where(f => f.IsExtensionMethods())
                                                             .Select(f => f.TypeNamespace))
                                           .ToArray();
        
        WriteUsings(_files.SelectMany(f => f.UsingStatements.Select(u => new UsingStatementData(u))).Where(u => !u.IsNamespace), extensionNameSpaces);

        if (_extensionTypes.Count != 0)
        {
            _writer.WriteUsing(GetExtensionNamespace());
            _writer.WriteLine();
        }
    }
    
    /// <summary>
    /// writes usings to the code writer
    /// </summary>
    private void WriteNamespaceUsings()
    {
        WriteUsings(_files.SelectMany(f => f.UsingStatements.Select(u => new UsingStatementData(u))).Where(u => u.IsNamespace), []);
    }

    private void WriteUsings(IEnumerable<UsingStatementData> usings, string[] extensionNameSpaces)
    {
        UsingStatementData[] uniqueUsings = usings
            .DistinctBy(u => u.UsingText)
            .Where(u => !_settings.Merge.IgnoreNameSpaces.Any(u.UsingName.StartsWith) && !extensionNameSpaces.Contains(u.UsingText))
            .OrderBy(u => u.UsingText)
            .ToArray();
        
        _writer.WriteUsings(uniqueUsings.Where(u => u.IsDefault).Select(u => u.UsingText));
        _writer.WriteUsings(uniqueUsings.Where(u => u.IsStatic).Select(u => u.UsingText));
        _writer.WriteUsings(uniqueUsings.Where(u => u.IsAlias).Select(u => u.UsingText));
    }

    /// <summary>
    /// Writes namespace to the code writer
    /// </summary>
    private void WriteNamespace(bool fileScoped)
    {
        _writer.WriteComment($"{_settings.Merge.PluginName} created with PluginMerge v({typeof(Program).Assembly.GetName().Version}) by MJSU @ https://github.com/dassjosh/Plugin.Merge");
        _writer.WriteNamespace(_settings.PlatformSettings.Namespace, fileScoped);
        _writer.WriteLine();
        if (!fileScoped)
        {
            _writer.WriteStartBracket();
        }
    }
    
    /// <summary>
    /// Writes namespace to the code writer
    /// </summary>
    private void WriteExtensionNamespace()
    {
        _writer.WriteLine();
        _writer.WriteNamespace(GetExtensionNamespace(), false);
        _writer.WriteLine();
        _writer.WriteStartBracket();
        if (_settings.Merge.CreatorMode != CreatorMode.MergeFramework)
        {
            WriteExtensionUsings();
        }
        _writer.WriteLine();
    }

    private void WriteExtensionUsings()
    {
        List<string> allTypes = new();
        foreach (FileHandler file in _files)
        {
            foreach (FileType type in file.FileTypes)
            {
                if (!type.IsExtensionMethods() && !allTypes.Contains(type.TypeName) && _settings.Merge.PluginName != type.TypeName)
                {
                    allTypes.Add(type.TypeName);
                }
            }
        }

        HashSet<string> addedTypes = new();
        foreach (FileType file in _extensionTypes)
        {
            foreach (string type in allTypes)
            {
                if (file.ContainsType(type) && addedTypes.Add(type))
                {
                    _writer.WriteIndent();
                    _writer.WriteUsingAlias(type, $"{_settings.Merge.PluginName}.{type}");
                }
            }
        }
    }

    /// <summary>
    /// Writes the start of the plugin class
    /// </summary>
    private void StartPluginClass()
    {
        bool isFramework = IsFrameworkMode || IsMergeFrameworkMode;
        _writer.WriteInfoAttribute(isFramework);
        _writer.WriteDescriptionAttribute(isFramework);
        _writer.WriteStartClass(_settings.Merge.PluginName, _plugin.PluginData.PluginBaseTypes, true);
        _writer.WriteStartBracket();
    }

    /// <summary>
    /// Writes the end of the plugin class
    /// </summary>
    private void EndPluginClass()
    {
        _writer.WriteEndBracket();
        _writer.WriteLine();
    }
        
    /// <summary>
    /// Writes the end of the namespace
    /// </summary>
    private void EndNamespace()
    {
        _writer.WriteEndBracket();
    }
        
    /// <summary>
    /// Writes the file to the plugin
    /// </summary>
    /// <param name="file"></param>
    private void Write(FileHandler file)
    {
        if (file.FileTypes.All(f => !f.HasCode()))
        {
            return;
        }

        if (file.FileTypes.All(f => f.IsExtensionMethods()))
        {
            return;
        }
        
        _writer.WriteStartRegion(file.RegionName);
        foreach (IFileType type in file.FileTypes.Where(type => type.HasCode() && !type.IsExtensionMethods()))
        {
            type.Write(_writer);
        }

        _writer.WriteEndRegion();
        _writer.WriteLine();
    }

    private void WritePluginFiles()
    {
        foreach (FileHandler file in _pluginFiles)
        {
            _logger.LogDebug("Writing plugin file: {Path}", file.FilePath);
            Write(file);
        }
    }    
    
    private void WriteDataFiles()
    {
        foreach (FileHandler file in _dataFiles)
        {
            _logger.LogDebug("Writing data file: {Path}", file.FilePath);
            Write(file);
        }
    }

    /// <summary>
    /// Writes the plugin and data files together in the order specified by the Order property
    /// </summary>
    private void WritePluginAndDataFiles()
    {
        List<FileHandler> orderedFiles = _pluginFiles.Concat(_dataFiles)
                                                     .OrderBy(f => f.Order)
                                                     .ToList();
        foreach (FileHandler file in orderedFiles)
        {
            _logger.LogDebug("Writing ordered file: {Path}", file.FilePath);
            Write(file);
        }
    }

    /// <summary>
    /// Writes the framework to the plugin
    /// </summary>
    private void WriteFrameworks()
    {
        foreach (FileHandler file in _frameworks)
        {
            _logger.LogDebug("Writing framework file: {File}", file.FilePath);
            _writer.WriteComment($"Framework {file.PluginData?.Title} v({file.PluginData?.Version}) by {file.PluginData?.Author}");
            _writer.WriteComment(file.PluginData?.Description);
            _writer.WriteStartRegion($"Merged Framework {file.PluginData?.Title}");
            _writer.WriteStartClass(_settings.Merge.PluginName, null, true);
            _writer.WriteStartBracket();
            foreach (FileType type in file.FileTypes.Where(f => f.HasCode() && !f.IsExtensionMethods()))
            {
                type.Write(_writer);
            }

            _writer.WriteEndBracket();
            _writer.WriteEndRegion();
            _writer.WriteLine();
        }
    }

    private void WriteExtensionMethods()
    {
        WriteExtensionNamespace();
        foreach (IFileType type in _extensionTypes)
        {
            _logger.LogDebug("Writing extension type: {Path}", type.TypeName);
            type.Write(_writer);
        }
        EndNamespace();
    }

    /// <summary>
    /// Returns the final output code
    /// </summary>
    /// <returns></returns>
    public string ToCode()
    {
        return _writer.GetCode();
    }
    
    private string GetExtensionNamespace()
    {
        return $"{_settings.PlatformSettings.Namespace}.{_settings.Merge.PluginName}Extensions";
    }
    
    
    private sealed class UsingStatementData
    {
        public readonly string UsingText;
        public readonly string UsingName;
        private readonly UsingType _type;

        public bool IsDefault => !IsStatic && !IsAlias;
        public bool IsStatic => _type.HasFlag(UsingType.Static);
        public bool IsAlias => _type.HasFlag(UsingType.Alias);
        public bool IsNamespace => _type.HasFlag(UsingType.Namespace);
        
        public UsingStatementData(UsingDirectiveSyntax @using)
        {
            UsingText = @using.ToString();
            if (UsingText.StartsWith("global ", StringComparison.Ordinal))
            {
                UsingText = UsingText[7..];
            }
            UsingName = @using.Name!.ToString();
            if (@using.StaticKeyword != default)
            {
                _type |= UsingType.Static;
            }
            if (@using.Alias is not null)
            {
                _type |= UsingType.Alias;
            }
            if(@using.Parent is BaseNamespaceDeclarationSyntax)
            {
                _type |= UsingType.Namespace;
            }
        }
    }

    [Flags]
    private enum UsingType
    {
        Default = 0,
        Static = 1 << 0,
        Alias = 1 << 1,
        Namespace = 1 << 2
    }
}