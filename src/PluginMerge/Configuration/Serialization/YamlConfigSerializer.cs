using YamlDotNet.Serialization;

namespace PluginMerge.Configuration.Serialization;

public class YamlConfigSerializer : IConfigSerializer
{
    public static readonly YamlConfigSerializer Instance = new ();
    private static readonly ISerializer Serializer = new SerializerBuilder().Build();
    private static readonly IDeserializer Deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();

    private YamlConfigSerializer()
    {
        
    }
    
    public string Serialize<T>(T data)
    {
        return Serializer.Serialize(data);
    }

    public T Deserialize<T>(string text)
    {
        return Deserializer.Deserialize<T>(text);
    }
}