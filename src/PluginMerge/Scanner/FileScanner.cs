namespace PluginMerge.Scanner;

public class FileScanner
{
    private readonly List<string> _inputPaths;
    private readonly string[] _ignorePaths;
    private readonly string[] _ignoreFiles;
    private readonly string _filter;
    private readonly ILogger _logger;

    private readonly string _objPath = Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar;
    private readonly string _binPath = Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar;
    
    public FileScanner(List<string> inputPaths, string filter = "*", IEnumerable<string> ignorePaths = null, IEnumerable<string> ignoreFiles = null)
    {
        ArgumentNullException.ThrowIfNull(inputPaths);
        ArgumentNullException.ThrowIfNull(filter);
        _logger = LogBuilder.GetLogger<FileScanner>();
        _filter = filter;
        _inputPaths = inputPaths;
        _ignorePaths = ignorePaths?.Select(p => p.ToFullPath()).ToArray() ?? Array.Empty<string>();
        _ignoreFiles = ignoreFiles?.Select(p => p.ToFullPath()).ToArray() ?? Array.Empty<string>();
    }

    public IEnumerable<ScannedFile> ScanFiles()
    {
        foreach (string path in _inputPaths.Select(p => p.ToFullPath()))
        {
            foreach (ScannedFile file in ScanPath(path))
            {
                yield return file;
            }
        }
    }

    private IEnumerable<ScannedFile> ScanPath(string dir)
    {
        if (!Directory.Exists(dir))
        {
            _logger.LogError("Directory does not exist: {Dir}", dir);
            yield break;
        }

        foreach (string file in Directory.GetFiles(dir, _filter, SearchOption.AllDirectories))
        {
            if (ShouldIgnorePath(file) || ShouldIgnoreFile(file))
            {
                continue;
            }

            yield return new ScannedFile(file, dir);
        }
    }

    private bool ShouldIgnorePath(string path)
    {
        return path.Contains(_objPath) || path.Contains(_binPath) || (_inputPaths.Count != 0 && _ignorePaths.Any(path.StartsWith));
    }

    private bool ShouldIgnoreFile(string file)
    {
        return _ignoreFiles.Length != 0 && _ignoreFiles.Contains(file);
    }
}