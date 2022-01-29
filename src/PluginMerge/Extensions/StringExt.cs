namespace PluginMerge.Extensions;

public static class StringExt
{
    public static string ToFullPath(this string path)
    {
        return Path.IsPathFullyQualified(path) ? path : Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), path));
    }
}