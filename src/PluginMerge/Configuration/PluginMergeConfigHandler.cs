using PluginMerge.Configuration.Serialization;

namespace PluginMerge.Configuration;

public class PluginMergeConfigHandler
{
    private readonly ILogger _logger;
    public static readonly PluginMergeConfigHandler Instance = new();

    private PluginMergeConfigHandler()
    {
        _logger = this.GetLogger();
    }
    
    public async Task Create(string file)
    {
        if (file == null) throw new ArgumentNullException(nameof(file));
        
        if (File.Exists(file))
        {
            _logger.LogError("Failed to create new merge config. File already exists at path: {File}", file);
            return;
        }

        PluginMergeConfig config = new();
        config.Initialize();
        
        await WriteConfig(file, config);

        _logger.LogInformation("Successfully created merge config at path: {File}", file);
    }
    
    public async Task<PluginMergeConfig> Load(string file)
    {
        if (file == null) throw new ArgumentNullException(nameof(file));
        
        if (!File.Exists(file))
        {
            _logger.LogError("Merge config file not found at path: {Path}. Please run with the -i/init argument to create a config file", file);
            return null;
        }

        PluginMergeConfig config = await ReadConfig(file);
        config.Initialize();
        await WriteConfig(file, config);

        return config;
    }

    private async Task WriteConfig(string file, PluginMergeConfig config)
    {
        IConfigSerializer serializer = GetSerializerForFile(file);
        await File.WriteAllTextAsync(file, serializer.Serialize(config));
    }
    
    private async Task<PluginMergeConfig> ReadConfig(string file)
    {
        IConfigSerializer serializer = GetSerializerForFile(file);
        string text = await File.ReadAllTextAsync(file);
        return serializer.Deserialize<PluginMergeConfig>(text);
    }

    private IConfigSerializer GetSerializerForFile(string file)
    {
        return Path.GetExtension(file).Equals(".json", StringComparison.OrdinalIgnoreCase) ? JsonConfigSerializer.Instance : YamlConfigSerializer.Instance;
    }
}