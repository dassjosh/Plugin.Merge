using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace PluginMerge.Configuration;

public class MergeConfig
{
    [JsonPropertyName("Plugin Name")]
    [YamlMember(Alias = "Plugin Name", Description = "Outputted plugin name")]
    public string PluginName { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonPropertyName("Creator Mode")]
    [YamlMember(Alias = "Creator Mode", Description = "Which type of file to output (Plugin, Framework, or MergeFramework)")]
    public CreatorMode CreatorMode { get; set; }
    
    [JsonPropertyName("Namespace Override")]
    [YamlMember(Alias = "Namespace Override", Description = "Overrides the default namespace")]
    public string NamespaceOverride { get; set; }
        
    [JsonPropertyName("Plugin Input Paths")]
    [YamlMember(Alias = "Plugin Input Paths", Description = "Paths to use when reading in source code relative to the merge config")]
    public List<string> InputPaths { get; set; }
        
    [JsonPropertyName("Plugin Output Paths")]
    [YamlMember(Alias = "Plugin Output Paths", Description = "Paths to use when writing the plugin file relative to the merge config")]
    public List<string> OutputPaths { get; set; }
    
    [YamlMember(Alias = "Reference Definitions", Description = "Oxide //References: definitions")]
    [JsonPropertyName("Reference Definitions")]
    public List<string> References { get; set; }
    
    [JsonPropertyName("Define Definitions")]
    [YamlMember(Alias = "Define Definitions", Description = "#define definitions")]
    public List<string> Defines { get; set; }

    [JsonPropertyName("Ignore Paths")]
    [YamlMember(Alias = "Ignore Paths", Description = "Paths to be ignored when searching for source code relative to merge config")]
    public List<string> IgnorePaths { get; set; }
    
    [JsonPropertyName("Ignore Files")]
    [YamlMember(Alias = "Ignore Files", Description = "Files to be ignored when searching for source code relative to merge config")]
    public List<string> IgnoreFiles { get; set; }
    
    [JsonPropertyName("Ignore Namespaces")]
    [YamlMember(Alias = "Ignore Namespaces", Description = "Namespaces to ignore when processing output file")]
    public List<string> IgnoreNameSpaces { get; set; }
    
    [JsonPropertyName("Code Style")]
    [YamlMember(Alias = "Code Style")]
    public CodeStyleConfig CodeStyle { get; set; }
    
    [JsonIgnore]
    [YamlIgnore]
    public IEnumerable<string> FinalFiles => OutputPaths.Select(p => Path.Combine(p, $"{PluginName}.cs").ToFullPath());

    private bool ShouldSerializeNamespaceOverride() => CreatorMode == CreatorMode.MergeFramework;

    public void Initialize()
    {
        PluginName ??= "MyPluginName";
        NamespaceOverride ??= string.Empty;
        InputPaths ??= new List<string> { "./" };
        OutputPaths ??= new List<string> {"./build"};
        Defines ??= new List<string> { "DEBUG" };
        References ??= new List<string>();
        IgnorePaths ??= new List<string>{"./IgnoreThisPath"};
        IgnoreFiles ??= new List<string>{"./IgnoreThisFile.cs"};
        IgnoreNameSpaces ??= new List<string> {"IgnoreThisNameSpace"};
        CodeStyle ??= new CodeStyleConfig();
        CodeStyle.Initialize();
    }
}