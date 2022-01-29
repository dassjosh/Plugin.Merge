namespace PluginMerge.Creator;

/// <summary>
/// Represents a plugin creator
/// </summary>
public class PluginCreator : BaseCreator
{
    /// <summary>
    /// Plugin Creator Constructor
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="files"></param>
    public PluginCreator(PluginMergeConfig settings, List<FileHandler> files) : base(settings, files)
    {
    }

    /// <summary>
    /// Creates a plugin
    /// </summary>
    public override bool Create()
    {
        if (!base.Create())
        {
            return false;
        }

        WriteReferences();
        WriteDefines();
        WriteUsings();
        WriteNamespace();
        StartPluginClass(true, false);
            
        foreach (FileHandler file in PluginFiles)
        {
            Logger.LogDebug("Writing plugin file: {Path}", file.FilePath);
            Write(file);
        }
            
        foreach (FileHandler file in DataFiles)
        {
            Logger.LogDebug("Writing data file: {Path}", file.FilePath);
            Write(file);
        }

        EndPluginClass();
        WriteFrameworks();
        EndNamespace();
        return true;
    }
}