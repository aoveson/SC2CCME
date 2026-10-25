using ModManager.StarCraft.Services;
using ModManager.StarCraft.Services.Log;
using ModManager.StarCraft.Services.Log.Enum;
using ModManager.StarCraft.Services.Log.Interface;
using ModManager.StarCraft.Services.Log.IO;
using ModManager.StarCraft.Services.Zip;
using ModManager.StarCraft.Services.Zip.Interface;

namespace ModManager.StarCraft.Services.UnitTest;

/// <summary>Verifies ZIP discovery, extraction, relocation, and failure logging.</summary>
public sealed class ZipServiceUnitTest : IDisposable
{
    private readonly IZipFileSystem _originalFileSystem = ZipService.FileSystem;

    /// <summary>Restores the ZIP seam and logger after each test.</summary>
    public void Dispose()
    {
        ZipService.FileSystem = _originalFileSystem;
        Service.Remove<Logger>();
    }

    /// <summary>Ignores files that do not use the ZIP extension.</summary>
    [Fact]
    public async Task IgnoresNonZipFilesWithoutTouchingThem()
    {
        FakeZipFileSystem fileSystem = new()
        {
            Files = ["campaign.txt"]
        };
        ZipService.FileSystem = fileSystem;
        RegisterConsoleLogger();

        await ZipService.UnzipZipsAsync("sc2");

        Assert.Empty(fileSystem.ExtractedArchives);
        Assert.Empty(fileSystem.DeletedFiles);
    }

    /// <summary>Extracts ZIP files, relocates the LOTV prologue, and deletes the archive.</summary>
    [Fact]
    public async Task ExtractsZipMovesPrologueAndDeletesArchive()
    {
        FakeZipFileSystem fileSystem = new()
        {
            Files = ["sc2/Maps/CustomCampaigns/campaign.ZIP"],
            Directories = ["sc2/Maps/CustomCampaigns/campaign/lotvprologue"]
        };
        ZipService.FileSystem = fileSystem;
        RegisterConsoleLogger();

        await ZipService.UnzipZipsAsync("sc2");

        Assert.Equal(fileSystem.Files[0], Assert.Single(fileSystem.ExtractedArchives).ArchivePath);
        Assert.Contains(("sc2/Maps/CustomCampaigns/campaign/lotvprologue", "sc2/Maps/Campaign/voidprologue"), fileSystem.MovedDirectories);
        Assert.Contains(fileSystem.Files[0], fileSystem.DeletedFiles);
    }

    /// <summary>Logs archive deletion failures without propagating the I/O exception.</summary>
    [Fact]
    public async Task DeleteFailureIsLoggedAndDoesNotEscape()
    {
        FakeZipFileSystem fileSystem = new()
        {
            Files = ["campaign.zip"],
            DeleteException = new IOException("locked")
        };
        ZipService.FileSystem = fileSystem;
        InMemoryLogWriter writer = new();
        Service.SetInstance(new Logger(writer, "memory", LogMethod.Notification));
        LogEntry? error = null;
        Service.Get<Logger>().LogNotification += (_, entry) => error = entry;

        await ZipService.UnzipZipsAsync("sc2");

        Assert.NotNull(error);
        Assert.Equal("I/O Error", error.LogTitle);
        Assert.Contains("locked", error.AdditionalInfo);
    }

    private static void RegisterConsoleLogger() => RegisterLogger(LogMethod.Console);

    private static void RegisterLogger(LogMethod method) =>
        Service.SetInstance(new Logger(new InMemoryLogWriter(), "memory", method));

    /// <summary>Records ZIP operations without touching the physical filesystem.</summary>
    private sealed class FakeZipFileSystem : IZipFileSystem
    {
        public string[] Files { get; init; } = [];

        public string[] Directories { get; init; } = [];

        public List<(string ArchivePath, string DestinationPath)> ExtractedArchives { get; } = [];

        public List<(string SourcePath, string DestinationPath)> MovedDirectories { get; } = [];

        public List<string> DeletedFiles { get; } = [];

        public IOException? DeleteException { get; init; }

        public string[] GetFiles(string path) => Files;

        public void SetNormalAttributes(string path)
        {
        }

        public Task ExtractToDirectoryAsync(string archivePath, string destinationPath)
        {
            ExtractedArchives.Add((archivePath, destinationPath));
            return Task.CompletedTask;
        }

        public string[] GetDirectories(string path, string searchPattern) => Directories;

        public void MoveDirectory(string sourcePath, string destinationPath) =>
            MovedDirectories.Add((sourcePath, destinationPath));

        public void DeleteFile(string path)
        {
            if(DeleteException != null)
            {
                throw DeleteException;
            }

            DeletedFiles.Add(path);
        }
    }

    /// <summary>Discards logger output for ZIP tests.</summary>
    private sealed class InMemoryLogWriter : ILogWriter
    {
        public LogCollection Read(string path) => new();

        public Task WriteAsync(LogCollection collection, string path) => Task.CompletedTask;
    }
}
