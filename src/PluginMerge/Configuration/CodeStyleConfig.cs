using System.Text.Json.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace PluginMerge.Configuration;

public class CodeStyleConfig
{
    [JsonPropertyName("Indent Character")]
    [YamlMember(Alias = "Indent Character", Description = "Character to use for code indents")]
    public char IndentCharacter { get; set; } = ' ';

    [JsonPropertyName("Indent Char Amount")]
    [YamlMember(Alias = "Indent Char Amount", Description = "The amount of characters to use when indenting once")]
    public int IndentAmount { get; set; } = 4;
    
    [JsonPropertyName("Indent Multiplier")]
    [YamlMember(Alias = "Indent Multiplier", Description = "Indent value will increase / decrease by this number")]
    public int IndentMultiplier { get; set; } = 1;
    
    [JsonPropertyName("New Line String")]
    [YamlMember(Alias = "New Line String", Description = "String to use for new lines in code", ScalarStyle = ScalarStyle.DoubleQuoted)]
    public string NewLine { get; set; }
    
    [JsonPropertyName("Write The Relative File Path In Region")]
    [YamlMember(Alias = "Write The Relative File Path In Region", Description = "Adds the code file path in a region")]
    public bool WriteFileRegion { get; set; } = true;
    
    [JsonPropertyName("Keep Comments")]
    [YamlMember(Alias = "Keep Code Comments", Description = "Adds the code file path in a region")]
    public bool KeepComments { get; set; } = true;

    public void Initialize()
    {
        NewLine ??= Environment.NewLine;
    }
}