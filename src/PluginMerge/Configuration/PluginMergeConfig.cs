using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace PluginMerge.Configuration;

public class PluginMergeConfig
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [YamlMember(Alias = "Platform", Description = "What platform to write the code file for (Oxide, uMod)")]
    public Platform Platform { get; set; }
    
    [JsonPropertyName("Merge Settings")]
    [YamlMember(Alias = "Merge Settings")]
    public MergeConfig Merge { get; set; }
    
    [JsonPropertyName("Compile Settings")]
    [YamlMember(Alias = "Compile Settings")]
    public CompileConfig Compile { get; set; }

    [JsonIgnore]
    [YamlIgnore]
    public PlatformSettings PlatformSettings { get; set; }

    public void Initialize()
    {
        Merge ??= new MergeConfig();
        Merge.Initialize();

        Compile ??= new CompileConfig();
        Compile.Initialize();
        
        PlatformSettings = string.IsNullOrEmpty(Merge.NamespaceOverride) ? PlatformSettings.GetPlatformSettings(Platform) : PlatformSettings.GetCustomPlatformSettings(Platform, Merge.NamespaceOverride);
    }
}