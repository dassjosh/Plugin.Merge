namespace PluginMerge.Creator;

/// <summary>
/// Represents a creator for frameworks
/// </summary>
public class FrameworkCreator : BaseCreator
{
    /// <summary>
    /// Framework Creator Constructor
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="files"></param>
    public FrameworkCreator(PluginMergeConfig settings, List<FileHandler> files) : base(settings, files)
    {
    }

    /// <summary>
    /// Creates a framework plugin
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
        StartPluginClass(true, true);
            
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