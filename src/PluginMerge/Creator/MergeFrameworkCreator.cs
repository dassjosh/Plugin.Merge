namespace PluginMerge.Creator;

/// <summary>
/// Represents a creator for frameworks
/// </summary>
public class MergeFrameworkCreator : BaseCreator
{
    /// <summary>
    /// Framework Creator Constructor
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="files"></param>
    public MergeFrameworkCreator(PluginMergeConfig settings, List<FileHandler> files) : base(settings, files)
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
        Writer.WriteFramework();
        StartPluginClass(true, true);
            
        foreach (FileHandler file in PluginFiles)
        {
            Logger.LogDebug("Writing plugin file: {Path}", file.FilePath);
            Write(file);
        }

        EndPluginClass();
            
        foreach (FileHandler file in DataFiles)
        {
            Logger.LogDebug("Writing data file: {Path}", file.FilePath);
            Write(file);
        }
            
        WriteFrameworks();
        EndNamespace();
        return true;
    }
}