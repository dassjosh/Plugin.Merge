namespace PluginMerge.Creator;

/// <summary>
/// 
/// </summary>
public class BaseCreator
{
    private readonly List<FileHandler> _files;
    private readonly PluginMergeConfig _settings;

    private FileHandler _plugin;
    /// <summary>
    /// Files that are part of the plugin
    /// </summary>
    protected readonly List<FileHandler> PluginFiles = new();
        
    /// <summary>
    /// Files that contain data types
    /// </summary>
    protected readonly List<FileHandler> DataFiles = new();
        
    /// <summary>
    /// Files that contain frameworks
    /// </summary>
    private readonly List<FileHandler> _frameworks = new();

    /// <summary>
    /// Code writer for the final file
    /// </summary>
    protected CodeWriter Writer;

    protected readonly ILogger Logger;

    /// <summary>
    /// Constructor for the base creator
    /// </summary>
    /// <param name="settings">Creator settings</param>
    /// <param name="files">Files that were processed</param>
    protected BaseCreator(PluginMergeConfig settings, List<FileHandler> files)
    {
        Logger = this.GetLogger();
        _files = files;
        _settings = settings;
    }

    /// <summary>
    /// Creates a PluginCreator or Framework created based on the mode
    /// </summary>
    /// <param name="settings">Plugin settings for the creator</param>
    /// <param name="files">Files to be processed</param>
    /// <returns></returns>
    public static BaseCreator CreateForMode(PluginMergeConfig settings, List<FileHandler> files)
    {
        return settings.Merge.CreatorMode switch
        {
            CreatorMode.Plugin => new PluginCreator(settings, files),
            CreatorMode.Framework => new FrameworkCreator(settings, files),
            CreatorMode.MergeFramework => new MergeFrameworkCreator(settings, files),
            _ => null
        };
    }

    /// <summary>
    /// Creates the CodeWriter for the plugin
    /// </summary>
    public virtual bool Create()
    {
        _plugin = FindPlugin();
        if (_plugin == null)
        {
            Logger.LogError("No Plugin Found in Files");
            return false;
        }

        FilterFiles(_plugin.PluginData);
        
        Writer = new CodeWriter(_plugin, _settings.Merge.CodeStyle, _settings.Merge.PluginName);
        return true;
    }

    /// <summary>
    /// Returns the file that has plugin declarations
    /// </summary>
    /// <returns></returns>
    private FileHandler FindPlugin()
    {
        return _files.FirstOrDefault(f => f.Type.HasFlag(FileSettings.Plugin));
    }

    /// <summary>
    /// Filters the files into their given types
    /// </summary>
    private void FilterFiles(PluginData data)
    {
        foreach (FileHandler file in _files)
        {
            if (file.Type.HasFlag(FileSettings.Framework))
            {
                _frameworks.Add(file);
                foreach (FileType type in file.FileTypes)
                {
                    if (type.TypeNamespace == data.NameSpace && type.TypeName == data.ClassName)
                    {
                        type.IsPluginType = true;
                    }
                }
                continue;
            }

            bool isPluginType = false;
            foreach (FileType type in file.FileTypes)
            {
                if (type.TypeNamespace == data.NameSpace && type.TypeName == data.ClassName)
                {
                    PluginFiles.Add(file);
                    type.IsPluginType = true;
                    isPluginType = true;
                }
            }

            if (isPluginType)
            {
                continue;
            }

            DataFiles.Add(file);
        }
    }
    
    /// <summary>
    /// writes usings to the code writer
    /// </summary>
    protected void WriteReferences()
    {
        Writer.WriteReferences(_settings.Merge.References);
    }
    
    /// <summary>
    /// writes usings to the code writer
    /// </summary>
    protected void WriteDefines()
    {
        Writer.WriteDefines(_files
                            .SelectMany(f => f.DefineDirectives)
                            .Concat(_settings.Merge.Defines)
                            .Distinct());
    }

    /// <summary>
    /// writes usings to the code writer
    /// </summary>
    protected void WriteUsings()
    {
        Writer.WriteUsings(_files
                           .SelectMany(f => f.UsingStatements)
                           .Distinct()
                           .Where(u => !_settings.Merge.IgnoreNameSpaces.Any(u.StartsWith)));
        
        Writer.WriteUsings(_files
                           .SelectMany(f => f.UsingAliases)
                           .Distinct());
    }

    /// <summary>
    /// Writes namespace to the code writer
    /// </summary>
    protected void WriteNamespace()
    {
        Writer.WriteComment($"{_settings.Merge.PluginName} created with PluginMerge v({typeof(Program).Assembly.GetName().Version}) by MJSU");
        Writer.WriteNamespace(_settings.PlatformSettings.Namespace);
        Writer.WriteLine();
        Writer.WriteStartBracket();
    }

    /// <summary>
    /// Writes the start of the plugin class
    /// </summary>
    /// <param name="isPartial"></param>
    /// <param name="isFramework"></param>
    protected void StartPluginClass(bool isPartial, bool isFramework)
    {
        Writer.WriteInfoAttribute(isFramework);
        Writer.WriteDescriptionAttribute(isFramework);
        Writer.WriteStartClass(_settings.Merge.PluginName, _settings.Merge.BaseClass, isPartial);
        Writer.WriteStartBracket();
    }

    /// <summary>
    /// Writes the end of the plugin class
    /// </summary>
    protected void EndPluginClass()
    {
        Writer.WriteEndBracket();
        Writer.WriteLine();
    }
        
    /// <summary>
    /// Writes the end of the namespace
    /// </summary>
    protected void EndNamespace()
    {
        Writer.WriteEndBracket();
    }
        
    /// <summary>
    /// Writes the file to the plugin
    /// </summary>
    /// <param name="file"></param>
    protected void Write(FileHandler file)
    {
        if (file.FileTypes.All(f => !f.HasCode()))
        {
            return;
        }
        
        Writer.WriteStartRegion(file.RegionName);
        foreach (FileType type in file.FileTypes.Where(type => type.HasCode()))
        {
            type.Write(Writer);
        }

        Writer.WriteEndRegion();
        Writer.WriteLine();
    }

    /// <summary>
    /// Writes the framework to the plugin
    /// </summary>
    protected void WriteFrameworks()
    {
        foreach (FileHandler file in _frameworks)
        {
            Logger.LogDebug("Writing framework file: {File}", file.FilePath);
            Writer.WriteComment($"Framework {file.PluginData?.Title} v({file.PluginData?.Version}) by {file.PluginData?.Author}");
            Writer.WriteComment(file.PluginData?.Description);
            Writer.WriteStartRegion($"Merged Framework {file.PluginData?.Title}");
            Writer.WriteStartClass(_settings.Merge.PluginName, null, true);
            Writer.WriteStartBracket();
            foreach (FileType type in file.FileTypes.Where(f => f.HasCode()))
            {
                type.Write(Writer);
            }

            Writer.WriteEndBracket();
            Writer.WriteEndRegion();
            Writer.WriteLine();
        }
    }

    /// <summary>
    /// Returns the final output code
    /// </summary>
    /// <returns></returns>
    public string ToCode()
    {
        return Writer.GetCode();
    }
}