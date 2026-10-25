using ModManager.StarCraft.Base.Interface;
using ModManager.StarCraft.Services;
using ModManager.StarCraft.Services.Log;
using ModManager.StarCraft.Services.Log.Enum;
using ModManager.StarCraft.Services.Log.IO;
using ModManager.StarCraft.Services.Settings;
using ModManager.StarCraft.Services.Settings.Interface;
using ModManager.StarCraft.Services.Updater.Enum;

namespace ModManager.StarCraft.Base.UnitTest;

/// <summary>
/// Owns process-wide Base test seams and restores them after each test fixture.
/// </summary>
internal sealed class TestContext : IDisposable
{
    public InMemoryFileSystem FileSystem { get; } = new();
    private readonly ISettingsStore _originalStore = ApplicationSettings.Store;

    /// <summary>Creates isolated settings, logging, and filesystem doubles.</summary>
    public TestContext()
    {
        Service.Remove<Logger>();
        ApplicationSettings.Store = new InMemorySettingsStore();
        ApplicationSettings.QuerySettings(settings =>
        {
            settings.Sc2Dir = "sc2";
            settings.Sc2Path = string.Empty;
            settings.UpdateSource = UpdateSourceOption.None;
            settings.ApplicationVersion = "test";
            settings.UseFileLogging = false;
        });
        CustomCampaignManager.FileSystem = FileSystem;
        Startup.FileSystem = FileSystem;
        Service.SetInstance(new Logger(new MemoryLogWriter(), "memory", LogMethod.Notification));
    }

    /// <summary>Restores production defaults and the original settings store.</summary>
    public void Dispose()
    {
        CustomCampaignManager.FileSystem = new PhysicalFileSystem();
        Startup.FileSystem = new PhysicalFileSystem();
        ApplicationSettings.Store = _originalStore;
        Service.Remove<Logger>();
        Service.Remove<ModManager.StarCraft.Services.Updater.VelopackUpdateService>();
    }
}

/// <summary>Stores application settings without using the XML settings file.</summary>
internal sealed class InMemorySettingsStore : ISettingsStore
{
    public ApplicationSettings Current { get; set; } = new();
    public ApplicationSettings Read() => Current;
    public void Write(ApplicationSettings settings) => Current = settings;
}

/// <summary>Discards log output while satisfying logger dependencies.</summary>
internal sealed class MemoryLogWriter : ModManager.StarCraft.Services.Log.Interface.ILogWriter
{
    public LogCollection Read(string path) => new();
    public Task WriteAsync(LogCollection collection, string path) => Task.CompletedTask;
}

/// <summary>Provides an in-memory implementation of the Base filesystem seam.</summary>
internal sealed class InMemoryFileSystem : IFileSystem
{
    private readonly Dictionary<string, string> _files = [];
    private readonly HashSet<string> _directories = new(StringComparer.OrdinalIgnoreCase);

    public void AddDirectory(string path)
    {
        string? current = Normalize(path);
        while(!string.IsNullOrEmpty(current))
        {
            _directories.Add(current);
            current = Path.GetDirectoryName(current)?.Replace('\\', '/');
        }
    }
    public void AddFile(string path, string contents)
    {
        string normalized = Normalize(path);
        _files[normalized] = contents;
        string? parent = Path.GetDirectoryName(normalized)?.Replace('\\', '/');
        while(!string.IsNullOrEmpty(parent))
        {
            _directories.Add(parent);
            parent = Path.GetDirectoryName(parent)?.Replace('\\', '/');
        }
    }
    public string ReadText(string path) => _files[Normalize(path)];
    public string[] GetFiles(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly) =>
        _files.Keys.Where(x => IsChild(path, x, searchOption) && Matches(Path.GetFileName(x), searchPattern)).ToArray();
    public string[] GetDirectories(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly) =>
        _directories.Where(x => IsChild(path, x, searchOption) && Matches(Path.GetFileName(x), searchPattern)).ToArray();
    public bool FileExists(string path) => _files.ContainsKey(Normalize(path));
    public bool DirectoryExists(string path) => _directories.Contains(Normalize(path));
    public void CreateDirectory(string path) => AddDirectory(path);
    public void DeleteFile(string path) => _files.Remove(Normalize(path));
    public void DeleteDirectory(string path, bool recursive)
    {
        string normalized = Normalize(path).TrimEnd('/');
        foreach(string file in _files.Keys.Where(x => x == normalized || x.StartsWith(normalized + "/", StringComparison.OrdinalIgnoreCase)).ToArray()) _files.Remove(file);
        foreach(string directory in _directories.Where(x => x == normalized || x.StartsWith(normalized + "/", StringComparison.OrdinalIgnoreCase)).ToArray()) _directories.Remove(directory);
    }
    public void CopyFile(string sourcePath, string destinationPath, bool overwrite) => AddFile(destinationPath, ReadText(sourcePath));
    public void MoveFile(string sourcePath, string destinationPath)
    {
        string contents = ReadText(sourcePath);
        DeleteFile(sourcePath);
        AddFile(destinationPath, contents);
    }
    public void MoveDirectory(string sourcePath, string destinationPath)
    {
        string source = Normalize(sourcePath).TrimEnd('/');
        string destination = Normalize(destinationPath).TrimEnd('/');
        foreach(string directory in _directories.Where(x => x == source || x.StartsWith(source + "/", StringComparison.OrdinalIgnoreCase)).ToArray())
        {
            _directories.Remove(directory);
            _directories.Add(destination + directory[source.Length..]);
        }
        foreach(string file in _files.Keys.Where(x => x.StartsWith(source + "/", StringComparison.OrdinalIgnoreCase)).ToArray())
        {
            string contents = _files[file];
            _files.Remove(file);
            _files[destination + file[source.Length..]] = contents;
        }
    }
    public async IAsyncEnumerable<string> ReadLinesAsync(string path)
    {
        foreach(string line in ReadText(path).Split('\n')) yield return line.TrimEnd('\r');
        await Task.CompletedTask;
    }
    private static string Normalize(string path) => path.Replace('\\', '/').TrimEnd('/');
    private static bool Matches(string value, string pattern) => pattern is "*" or "*.*" || (pattern.StartsWith("*.") && value.EndsWith(pattern[1..], StringComparison.OrdinalIgnoreCase)) || string.Equals(value, pattern, StringComparison.OrdinalIgnoreCase);
    private static bool IsChild(string parent, string child, SearchOption option)
    {
        string normalized = Normalize(parent);
        if(!child.StartsWith(normalized + "/", StringComparison.OrdinalIgnoreCase))
            return false;
        string relative = child[(normalized.Length + 1)..];
        return option == SearchOption.AllDirectories || !relative.Contains('/');
    }
}
