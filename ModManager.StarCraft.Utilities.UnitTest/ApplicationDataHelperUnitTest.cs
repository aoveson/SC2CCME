using ModManager.StarCraft.Utilities;
using ModManager.StarCraft.Utilities.Interface;

namespace ModManager.StarCraft.Utilities.UnitTest;

/// <summary>
/// Verifies application-data path resolution through the filesystem seam.
/// </summary>
public sealed class ApplicationDataHelperUnitTest
{
    /// <summary>Creates the application-data directory through the configured filesystem.</summary>
    [Fact]
    public void ApplicationDataPathCanBeCreated()
    {
        InMemoryFileSystem fileSystem = new();
        IApplicationDataFileSystem original = ApplicationDataHelper.FileSystem;
        ApplicationDataHelper.FileSystem = fileSystem;
        try
        {
            string path = ApplicationDataHelper.GetCCMDataPathCreateIfNotExists();
            Assert.Equal(ApplicationDataHelper.CCMDataPath, path);
            Assert.Contains(path, fileSystem.CreatedDirectories);
        }
        finally
        {
            ApplicationDataHelper.FileSystem = original;
        }
    }

    /// <summary>Falls back to the current directory when the special-folder provider fails.</summary>
    [Fact]
    public void ApplicationDataPathFallsBackWhenSpecialFolderLookupFails()
    {
        Func<Environment.SpecialFolder, string> originalProvider = ApplicationDataHelper.SpecialFolderPathProvider;
        IApplicationDataFileSystem originalFileSystem = ApplicationDataHelper.FileSystem;
        try
        {
            ApplicationDataHelper.SpecialFolderPathProvider = _ => throw new InvalidOperationException();
            ApplicationDataHelper.FileSystem = new InMemoryFileSystem();

            Assert.Equal(
                Path.Combine(Environment.CurrentDirectory, ApplicationDataHelper.CCM_DATA_DIRECTORY_NAME),
                ApplicationDataHelper.CCMDataPath);
        }
        finally
        {
            ApplicationDataHelper.SpecialFolderPathProvider = originalProvider;
            ApplicationDataHelper.FileSystem = originalFileSystem;
        }
    }

    private sealed class InMemoryFileSystem : IApplicationDataFileSystem
    {
        public List<string> CreatedDirectories { get; } = [];

        public bool DirectoryExists(string path) => CreatedDirectories.Contains(path);

        public void CreateDirectory(string path) => CreatedDirectories.Add(path);
    }
}
