namespace PluginMerge.Configuration.Serialization;

public interface IConfigSerializer
{
    public string Serialize<T>(T data);
    public T Deserialize<T>(string text);
}