using Microsoft.CodeAnalysis.CSharp;

namespace PluginMerge.Configuration;

/// <summary>
/// Represents the settings for a platform.
/// </summary>
public class PlatformSettings
{
    private static readonly PlatformSettings Oxide = new("Oxide.Plugins", LanguageVersion.CSharp6);
    private static readonly PlatformSettings uMod = new("uMod.Plugins", LanguageVersion.CSharp9);

    /// <summary>
    /// Represents the lang version the chosen platform supports
    /// </summary>
    public LanguageVersion Lang { get; }
        
    /// <summary>
    /// Represents the namespace that should be used for the chosen platform
    /// </summary>
    public string Namespace { get; }

    /// <summary>
    /// Platform Settings Constructor
    /// </summary>
    /// <param name="namespace"></param>
    /// <param name="lang"></param>
    private PlatformSettings(string @namespace, LanguageVersion lang)
    {
        Namespace = @namespace;
        Lang = lang;
    }

    /// <summary>
    /// Returns the platform settings for a given platform
    /// </summary>
    /// <param name="platform"></param>
    /// <returns></returns>
    public static PlatformSettings GetPlatformSettings(Platform platform)
    {
        return platform switch
        {
            Platform.Oxide => Oxide,
            Platform.uMod => uMod,
            _ => null
        };
    }
}