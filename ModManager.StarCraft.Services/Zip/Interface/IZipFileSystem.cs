namespace ModManager.StarCraft.Services.Zip.Interface;

/// <summary>
/// Provides filesystem operations required by ZIP extraction.
/// </summary>
public interface IZipFileSystem
{
    /// <summary>
    /// Gets files from a directory.
    /// </summary>
    string[] GetFiles(string path);

    /// <summary>
    /// Sets a file's attributes to normal.
    /// </summary>
    void SetNormalAttributes(string path);

    /// <summary>
    /// Extracts an archive into a directory.
    /// </summary>
    Task ExtractToDirectoryAsync(string archivePath, string destinationPath);

    /// <summary>
    /// Finds directories matching a pattern.
    /// </summary>
    string[] GetDirectories(string path, string searchPattern);

    /// <summary>
    /// Moves a directory.
    /// </summary>
    void MoveDirectory(string sourcePath, string destinationPath);

    /// <summary>
    /// Deletes a file.
    /// </summary>
    void DeleteFile(string path);
}