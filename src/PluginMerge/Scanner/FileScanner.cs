namespace PluginMerge.Scanner;

public class FileScanner
{
    private readonly List<string> _paths;
    private readonly List<string> _ignorePaths;
    private readonly List<string> _ignoreFiles;
    private readonly string _filter;
    private readonly List<ScannedFile> _files = new();
    private readonly ILogger _logger;
    
    public FileScanner(List<string> paths, string filter = "*", IEnumerable<string> ignorePaths = null, IEnumerable<string> ignoreFiles = null)
    {
        ArgumentNullException.ThrowIfNull(paths);
        ArgumentNullException.ThrowIfNull(filter);
        _paths = paths;
        _filter = filter;
        _ignorePaths = ignorePaths?.Select(p => p.ToFullPath()).ToList();
        _ignoreFiles = ignoreFiles?.Select(p => p.ToFullPath()).ToList();
        _logger = this.GetLogger();
    }

    public List<ScannedFile> ScanFiles()
    {
        foreach (string path in _paths.Select(p => p.ToFullPath()))
        {
            ScanPath(path, path);
        }

        return _files;
    }

    private void ScanPath(string dir, string root)
    {
        if (!Directory.Exists(dir))
        {
            _logger.LogError("Directory does not exist: {Dir}", dir);
            return;
        }

        foreach (string file in Directory.GetFiles(dir, _filter, SearchOption.AllDirectories))
        {
            if (ShouldIgnorePath(file) || ShouldIgnoreFile(file))
            {
                continue;
            }
            
            _files.Add(new ScannedFile(file, root));
        }
    }

    private bool ShouldIgnorePath(string path)
    {
        return path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") 
               || path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}") 
               || (_ignorePaths?.Any(path.StartsWith) ?? false);
    }

    private bool ShouldIgnoreFile(string file)
    {
        return _ignoreFiles != null && _ignoreFiles.Contains(file);
    }
}