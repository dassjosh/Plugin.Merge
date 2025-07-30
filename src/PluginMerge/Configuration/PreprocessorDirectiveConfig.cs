using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace PluginMerge.Configuration;

public class PreprocessorDirectiveConfig
{
    [JsonPropertyName("Directive")]
    [YamlMember(Alias = "Directive", Description = "The Directive That Is Required By The Plugin")]
    public string Directive { get; set; }
    
    [JsonPropertyName("Error Message")]
    [YamlMember(Alias = "Error Message", Description = "The Compiler Error Message Show When The Directive Is Missing")]
    public string Message { get; set; }
    
    [JsonPropertyName("Enabled")]
    [YamlMember(Alias = "Enabled", Description = "If This Directive Is Enabled")]
    public bool Enabled { get; set; }
}