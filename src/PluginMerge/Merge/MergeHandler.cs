using System.Diagnostics;

namespace PluginMerge.Merge;

public class MergeHandler
{
    private readonly PluginMergeConfig _config;
    private readonly MergeConfig _merge;
    private readonly ILogger _logger;
    private List<FileHandler> _files = new();

    public MergeHandler(PluginMergeConfig config)
    {
        _config = config;
        _merge = config.Merge;
        _logger = this.GetLogger();
    }

    public async Task Run()
    {
        Stopwatch sw = Stopwatch.StartNew();
        _logger.LogInformation("Starting Plugin Merge Mode: {Mode}", _merge.CreatorMode);
        _logger.LogInformation("Input Paths: {Input}", string.Join(", ", _merge.InputPaths.Select(p => p.ToFullPath())));

        foreach (string path in _merge.OutputPaths)
        {
            if (!Directory.Exists(path))
            {
                _logger.LogDebug("Output path doesn't exist. Creating output path: {Path}", path);
                Directory.CreateDirectory(path);
            }
        
            _merge.IgnorePaths.Add(path);
        }
        
        FileScanner scanner = new(_merge.InputPaths, "*.cs", _merge.IgnorePaths, _merge.IgnoreFiles);
        foreach (ScannedFile file in scanner.ScanFiles())
        {
            _files.Add(new FileHandler(file));
        }

        await Task.WhenAll(_files.Select(f => f.ProcessFile(_config.PlatformSettings)));

        _files = _files.Where(f => !f.Type.HasFlag(FileSettings.Exclude)).OrderBy(f => f.Order).ToList();

        BaseCreator creator = BaseCreator.CreateForMode(_config, _files);
        if (!creator.Create())
        {
            return;
        }

        List<string> finalFiles = _merge.FinalFiles.ToList();
        foreach (string file in finalFiles)
        {
            await File.WriteAllTextAsync(file, creator.ToCode());
        }

        sw.Stop();
        _logger.LogInformation("Merged completed successfully in {Time}ms. {Mode} {Name} saved to: {File}", sw.ElapsedMilliseconds, _merge.CreatorMode, _merge.PluginName, string.Join(", ", finalFiles));
    }
}