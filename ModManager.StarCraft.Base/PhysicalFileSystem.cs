namespace ModManager.StarCraft.Base;

/// <summary>
/// Performs filesystem operations against the host operating system.
/// </summary>
public sealed class PhysicalFileSystem : Interface.IFileSystem
{
    /// <inheritdoc />
    public string[] GetFiles(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly) =>
        Directory.GetFiles(path, searchPattern, searchOption);
    /// <inheritdoc />
    public string[] GetDirectories(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly) =>
        Directory.GetDirectories(path, searchPattern, searchOption);
    /// <inheritdoc />
    public bool FileExists(string path) => File.Exists(path);
    /// <inheritdoc />
    public bool DirectoryExists(string path) => Directory.Exists(path);
    /// <inheritdoc />
    public void CreateDirectory(string path) => Directory.CreateDirectory(path);
    /// <inheritdoc />
    public void DeleteFile(string path) => File.Delete(path);
    /// <inheritdoc />
    public void DeleteDirectory(string path, bool recursive) => Directory.Delete(path, recursive);
    /// <inheritdoc />
    public void CopyFile(string sourcePath, string destinationPath, bool overwrite) => File.Copy(sourcePath, destinationPath, overwrite);
    /// <inheritdoc />
    public void MoveFile(string sourcePath, string destinationPath) => File.Move(sourcePath, destinationPath);
    /// <inheritdoc />
    public void MoveDirectory(string sourcePath, string destinationPath) => Directory.Move(sourcePath, destinationPath);
    /// <inheritdoc />
    public IAsyncEnumerable<string> ReadLinesAsync(string path) => File.ReadLinesAsync(path);
}