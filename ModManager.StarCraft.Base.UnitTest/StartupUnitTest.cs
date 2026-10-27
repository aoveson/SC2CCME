using System.Reflection;
using ModManager.StarCraft.Base.Enum;
using ModManager.StarCraft.Services.Settings;
using ModManager.StarCraft.Utilities.Interface;

namespace ModManager.StarCraft.Base.UnitTest;

/// <summary>
/// Verifies startup progression, directory creation, and executable-location reporting.
/// </summary>
public sealed class StartupUnitTest : IDisposable
{
    private readonly TestContext _context = new();

    /// <summary>Restores the shared startup test state.</summary>
    public void Dispose() => _context.Dispose();

    /// <summary>Raises completion when the startup step list is exhausted.</summary>
    [Fact]
    public async Task StartupRunsThePipelineAndRaisesCompletionEvents()
    {
        Startup startup = CreateStartup([() => Task.CompletedTask]);
        int completed = 0;
        startup.StartupCompleted += (_, _) => completed++;

        await startup.ContinueStartupAsync();

        Assert.Equal(1, completed);
    }

    /// <summary>Creates the configured campaign directory tree through the filesystem seam.</summary>
    [Fact]
    public async Task VerifyDirectoriesCreatesTheConfiguredTree()
    {
        Startup startup = CreateStartup();
        SetSettings(sc2Path: "sc2/StarCraft II.exe");
        _context.FileSystem.AddFile("sc2/StarCraft II.exe", string.Empty);
        List<string> created = [];
        Startup.FileSystem = new RecordingFileSystem(_context.FileSystem, created);

        await InvokeAsync(startup, "VerifyDirectoriesAsync");

        Assert.Contains("sc2/Maps/Campaign", created);
        Assert.Contains("sc2/Maps/CustomCampaigns", created);
        Assert.Contains("sc2/Mods", created);
    }

    /// <summary>Reports both missing and existing StarCraft II executable paths.</summary>
    [Fact]
    public async Task VerifyLocationReportsMissingAndExistingExecutables()
    {
        Startup startup = CreateStartup();
        VerifyStarCraftLocationResult? result = null;
        startup.VerifyStarCraftLocationCompleted += (_, value) => result = value;

        SetSettings(sc2Path: "sc2/missing.exe");
        await InvokeAsync(startup, "VerifyStarCraftLocationAsync");
        Assert.Equal(VerifyStarCraftLocationResult.DoesNotExist, result);

        _context.FileSystem.AddFile("sc2/found.exe", string.Empty);
        SetSettings(sc2Path: "sc2/found.exe");
        await InvokeAsync(startup, "VerifyStarCraftLocationAsync");
        Assert.Equal(VerifyStarCraftLocationResult.Success, result);
    }

    /// <summary>Creates a startup instance with deterministic private workflow state.</summary>
    private Startup CreateStartup(List<Func<Task>>? steps = null)
    {
        Startup startup = (Startup)Activator.CreateInstance(typeof(Startup), nonPublic: true)!;
        typeof(Startup).GetField("<Platform>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(startup, ExecutingPlatform.Linux);
        typeof(Startup).GetField("<StartupSteps>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(startup, steps ?? [() => Task.CompletedTask]);
        typeof(Startup).GetField("_startupStep", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(startup, 0);
        return startup;
    }

    /// <summary>Updates the configured executable path in the in-memory settings store.</summary>
    private static void SetSettings(string sc2Path) => ApplicationSettings.QuerySettings(settings => settings.Sc2Path = sc2Path);

    /// <summary>Invokes a private startup step while keeping the test focused on that step.</summary>
    private static async Task InvokeAsync(Startup startup, string methodName) =>
        await (Task)typeof(Startup).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(startup, null)!;

    /// <summary>Records directory creation while delegating all other operations to the fake filesystem.</summary>
    private sealed class RecordingFileSystem(InMemoryFileSystem inner, List<string> created) : IFileSystem
    {
        public string[] GetFiles(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly) => inner.GetFiles(path, searchPattern, searchOption);
        public string[] GetDirectories(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly) => inner.GetDirectories(path, searchPattern, searchOption);
        public bool FileExists(string path) => inner.FileExists(path);
        public bool DirectoryExists(string path) => inner.DirectoryExists(path);
        public void CreateDirectory(string path) { created.Add(path); inner.CreateDirectory(path); }
        public void DeleteFile(string path) => inner.DeleteFile(path);
        public void DeleteDirectory(string path, bool recursive) => inner.DeleteDirectory(path, recursive);
        public void CopyFile(string sourcePath, string destinationPath, bool overwrite) => inner.CopyFile(sourcePath, destinationPath, overwrite);
        public void MoveFile(string sourcePath, string destinationPath) => inner.MoveFile(sourcePath, destinationPath);
        public void MoveDirectory(string sourcePath, string destinationPath) => inner.MoveDirectory(sourcePath, destinationPath);
        public IAsyncEnumerable<string> ReadLinesAsync(string path) => inner.ReadLinesAsync(path);
        public async Task<byte[]> ReadAllBytesAsync(string path) => await inner.ReadAllBytesAsync(path);
        public Task WriteAllTextAsync(string path, string text)
        {
            throw new NotImplementedException();
        } 
    }
}
