namespace PluginMerge.Enums;

/// <summary>
/// File Types parsed from the file
/// </summary>
[Flags]
public enum FileSettings
{
    /// <summary>
    /// File inherits from Plugin class
    /// </summary>
    Plugin = 1 << 0,

    /// <summary>
    /// File defines itself as a framework
    /// </summary>
    Framework = 1 << 1,
        
    /// <summary>
    /// File wants to be excluded
    /// </summary>
    Exclude = 1 << 2
}