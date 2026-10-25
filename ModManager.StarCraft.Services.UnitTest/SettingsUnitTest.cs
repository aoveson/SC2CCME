using ModManager.StarCraft.Services;
using ModManager.StarCraft.Services.Settings;
using ModManager.StarCraft.Services.Settings.Interface;
using ModManager.StarCraft.Services.Updater.Enum;
using System.Reflection;

namespace ModManager.StarCraft.Services.UnitTest;

/// <summary>Verifies settings persistence, defaults, change tracking, and query behavior.</summary>
public sealed class SettingsUnitTest : IDisposable
{
    private readonly ISettingsStore _originalStore = ApplicationSettings.Store;
    private readonly Func<string> _originalDataPathFactory = ApplicationSettings.SettingsDataPathFactory;
    private readonly ModManager.StarCraft.Utilities.Interface.IApplicationDataFileSystem _originalApplicationDataFileSystem =
        ModManager.StarCraft.Utilities.ApplicationDataHelper.FileSystem;

    /// <summary>Configures an isolated settings path and application-data filesystem.</summary>
    public SettingsUnitTest()
    {
        ApplicationSettings.SettingsDataPathFactory = () => "memory";
        ModManager.StarCraft.Utilities.ApplicationDataHelper.FileSystem = new InMemoryApplicationDataFileSystem();
    }

    /// <summary>Restores the process-wide settings and filesystem seams.</summary>
    public void Dispose()
    {
        ApplicationSettings.Store = _originalStore;
        ApplicationSettings.SettingsDataPathFactory = _originalDataPathFactory;
        ModManager.StarCraft.Utilities.ApplicationDataHelper.FileSystem = _originalApplicationDataFileSystem;
    }

    /// <summary>Persists changes made through the action-based settings query.</summary>
    [Fact]
    public void QuerySettingsReadsAndWritesThroughTheConfiguredStore()
    {
        InMemorySettingsStore store = new()
        {
            Current = new ApplicationSettings { ApplicationVersion = "before" }
        };
        ApplicationSettings.Store = store;

        ApplicationSettings.QuerySettings((Action<ApplicationSettings>)(settings => settings.ApplicationVersion = "after"));

        Assert.Equal("after", store.Current.ApplicationVersion);
        Assert.Equal(1, store.WriteCount);
    }

    /// <summary>Does not persist settings when the action makes no changes.</summary>
    [Fact]
    public void QuerySettingsDoesNotWriteWhenTheActionDoesNotChangeState()
    {
        InMemorySettingsStore store = new();
        ApplicationSettings.Store = store;

        ApplicationSettings.QuerySettings(_ => { });

        Assert.Equal(0, store.WriteCount);
    }

    /// <summary>Returns a selected setting without writing unchanged state.</summary>
    [Fact]
    public void GenericQueryReturnsTheSelectedValue()
    {
        InMemorySettingsStore store = new()
        {
            Current = new ApplicationSettings { Sc2Dir = "campaigns" }
        };
        ApplicationSettings.Store = store;

        string value = ApplicationSettings.QuerySettings(settings => settings.Sc2Dir);

        Assert.Equal("campaigns", value);
        Assert.Equal(0, store.WriteCount);
    }

    /// <summary>Handles null delegates and stores that return no settings.</summary>
    [Fact]
    public void NullQueryDelegatesReturnWithoutWriting()
    {
        InMemorySettingsStore store = new();
        ApplicationSettings.Store = store;

        ApplicationSettings.QuerySettings((Action<ApplicationSettings>)null!);
        string value = ApplicationSettings.QuerySettings((Func<ApplicationSettings, string>)null!);
        store.ReturnNull = true;
        ApplicationSettings.QuerySettings((Action<ApplicationSettings>)(_ => { }));

        Assert.Null(value);
        Assert.Equal(0, store.WriteCount);
        Assert.Equal(Path.Combine("memory", ApplicationSettings.SETTINGS_FILE_NAME), ApplicationSettings.SettingsFilePath);
    }

    /// <summary>Preserves default values and tracks property changes.</summary>
    [Fact]
    public void SettingsPropertiesTrackChangesAndPreserveDefaults()
    {
        ApplicationSettings settings = new();

        Assert.True(settings.UseFileLogging);
        Assert.Equal(5, settings.FileLoggingMaxMegabytes);
        Assert.Equal("v0.0.0.0", settings.ApplicationVersion);
        Assert.False(settings.UsePreReleaseUpdates);
        Assert.Equal("https://github.com/aoveson/SC2CCME", settings.GithubUpdateRepo);

        settings.UseFileLogging = false;
        settings.FileLoggingMaxMegabytes = 10;
        settings.UsePreReleaseUpdates = true;
        settings.GithubUpdateRepo = "repo";
        settings.Sc2Path = "sc2.exe";
        settings.Sc2Dir = "directory";

        Assert.False(settings.UseFileLogging);
        Assert.Equal(10, settings.FileLoggingMaxMegabytes);
        Assert.True(settings.UsePreReleaseUpdates);
        Assert.Equal("repo", settings.GithubUpdateRepo);
        Assert.Equal("sc2.exe", settings.Sc2Path);
        Assert.Equal("directory", settings.Sc2Dir);
    }

    /// <summary>Preserves and tracks update-source settings.</summary>
    [Fact]
    public void UpdateSourcePropertiesTrackChangesAndPreserveValues()
    {
        ApplicationSettings settings = new();

        Assert.Equal(UpdateSourceOption.Github, settings.UpdateSource);
        Assert.False(settings.PendingUpdate);
        Assert.Null(settings.WebUpdateSourceUri);

        settings.UpdateSource = UpdateSourceOption.Directory;
        settings.PendingUpdate = true;
        settings.WebUpdateSourceUri = "https://example.test/update";
        settings.DirectoryUpdateSourcePath = "updates";

        Assert.Equal(UpdateSourceOption.Directory, settings.UpdateSource);
        Assert.True(settings.PendingUpdate);
        Assert.Equal("https://example.test/update", settings.WebUpdateSourceUri);
        Assert.Equal("updates", settings.DirectoryUpdateSourcePath);

    }

    private sealed class InMemoryApplicationDataFileSystem : ModManager.StarCraft.Utilities.Interface.IApplicationDataFileSystem
    {
        public bool DirectoryExists(string path) => false;
        public void CreateDirectory(string path) { }
    }

    private sealed class InMemorySettingsStore : ISettingsStore
    {
        public ApplicationSettings Current { get; set; } = new();

        public int WriteCount { get; set; }

        public bool ReturnNull { get; set; }

        public ApplicationSettings Read()
        {
            if(ReturnNull)
            {
                return null!;
            }

            typeof(ApplicationSettings)
                .GetField("_settingsUpdated", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(Current, false);
            return Current;
        }

        public void Write(ApplicationSettings settings)
        {
            Current = settings;
            WriteCount++;
        }

    }
}
