using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis;
using YamlDotNet.Serialization;

namespace PluginMerge.Configuration;

public class CompileConfig
{
    [YamlMember(Alias = "Ignore Paths")]
    [JsonPropertyName("Assembly Paths")]
    public List<string> AssemblyPaths { get; set; }
    
    [JsonPropertyName("Ignore Paths")]
    [YamlMember(Alias = "Ignore Paths", Description = "Ignores the following paths relative to the merge config")]
    public List<string> IgnorePaths { get; set; }
    
    [JsonPropertyName("Ignore Files")]
    [YamlMember(Alias = "Ignore Files", Description = "Ignores the following files relative to the merge config")]
    public List<string> IgnoreFiles { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonPropertyName("Compile Log Level (Hidden, Info, Warning, Error)")]
    [YamlMember(Alias = "Compile Log Level (Hidden, Info, Warning, Error)")]
    public DiagnosticSeverity CompileLogLevel { get; set; } = DiagnosticSeverity.Error;
    
    public void Initialize()
    {
        AssemblyPaths ??= new List<string> { "./Assemblies" };
        IgnorePaths ??= new List<string> { "./Assemblies/x86", "./Assemblies/x64" };
        IgnoreFiles ??= new List<string> {"./Assemblies/Newtonsoft.Json.dll"};
    }
}