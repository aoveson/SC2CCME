using System.IO.Compression;
using ModManager.StarCraft.Services.Zip.Interface;

namespace ModManager.StarCraft.Services.Zip;

/// <summary>
/// Performs ZIP operations against the physical filesystem.
/// </summary>
public sealed class PhysicalZipFileSystem : IZipFileSystem
{
    /// <inheritdoc />
    public string[] GetFiles(string path) => Directory.GetFiles(path);

    /// <inheritdoc />
    public void SetNormalAttributes(string path) => File.SetAttributes(path, FileAttributes.Normal);

    /// <inheritdoc />
    public async Task ExtractToDirectoryAsync(string archivePath, string destinationPath)
        => await ZipFile.ExtractToDirectoryAsync(archivePath, destinationPath, true);

    /// <inheritdoc />
    public string[] GetDirectories(string path, string searchPattern) =>
        Directory.GetDirectories(path, searchPattern, SearchOption.AllDirectories);

    /// <inheritdoc />
    public void MoveDirectory(string sourcePath, string destinationPath) => Directory.Move(sourcePath, destinationPath);

    /// <inheritdoc />
    public void DeleteFile(string path) => File.Delete(path);
}