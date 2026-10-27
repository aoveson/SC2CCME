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
        IFileSystem original = ApplicationDataHelper.FileSystem;
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
        IFileSystem originalFileSystem = ApplicationDataHelper.FileSystem;
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

    private sealed class InMemoryFileSystem : IFileSystem
    {
        public List<string> CreatedDirectories { get; } = [];

        public bool DirectoryExists(string path) => CreatedDirectories.Contains(path);

        public void CreateDirectory(string path) => CreatedDirectories.Add(path);

        public string[] GetFiles(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            throw new NotImplementedException();
        }

        public string[] GetDirectories(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            throw new NotImplementedException();
        }

        public bool FileExists(string path)
        {
            throw new NotImplementedException();
        }

        public void DeleteFile(string path)
        {
            throw new NotImplementedException();
        }

        public void DeleteDirectory(string path, bool recursive)
        {
            throw new NotImplementedException();
        }

        public void CopyFile(string sourcePath, string destinationPath, bool overwrite)
        {
            throw new NotImplementedException();
        }

        public void MoveFile(string sourcePath, string destinationPath)
        {
            throw new NotImplementedException();
        }

        public void MoveDirectory(string sourcePath, string destinationPath)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<string> ReadLinesAsync(string path)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> ReadAllBytesAsync(string path)
        {
            throw new NotImplementedException();
        }

        public Task WriteAllTextAsync(string path, string text)
        {
            throw new NotImplementedException();
        } 
    }
}
