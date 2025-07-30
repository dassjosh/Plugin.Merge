namespace PluginMerge.Merge;

public interface IFileType
{
    string TypeName { get; }
    string TypeNamespace { get; }
    
    void Write(CodeWriter writer);

    bool HasCode();

    void AddSettings(FileSettings settings);
    
    bool IsPlugin();

    bool IsExtensionMethods();
}