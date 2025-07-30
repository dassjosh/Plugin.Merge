using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace PluginMerge.Configuration;

public class PreprocessorDirectives
{
    [JsonPropertyName("Preprocessor Directives")]
    [YamlMember(Alias = "Preprocessor Directives", Description = "Preprocessor Directives that are required to build the plugin")]
    public List<PreprocessorDirectiveConfig> Directives { get; set; }

    [JsonIgnore]
    [YamlIgnore]
    public bool HasDirectives => EnabledDirectives.Count != 0;

    [JsonIgnore]
    [YamlIgnore]
    public List<PreprocessorDirectiveConfig> EnabledDirectives { get; private set; }
    
    public void Initialize()
    {
        Directives ??= new List<PreprocessorDirectiveConfig>
        {
            new()
            {
                Directive = "OXIDE",
                Message = "This plugin requires OXIDE",
                Enabled = false
            }
        };
        
        EnabledDirectives = Directives.Where(d => d.Enabled).ToList();
    }
}