using System.Text.Json;

namespace PluginMerge.Configuration.Serialization;

public class JsonConfigSerializer : IConfigSerializer
{
    public static readonly JsonConfigSerializer Instance = new();
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    private JsonConfigSerializer()
    {
        
    }
    
    public string Serialize<T>(T data)
    {
        return JsonSerializer.Serialize(data, Options);
    }

    public T Deserialize<T>(string text)
    {
        return JsonSerializer.Deserialize<T>(text);
    }
}