namespace ModManager.StarCraft.Utilities.Interface;

/// <summary>
/// Provides filesystem operations used by campaign management and startup workflows.
/// </summary>
public interface IFileSystem
{
    /// <summary>
    /// Gets files below a path.
    /// </summary>
    string[] GetFiles(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly);

    /// <summary>
    /// Gets directories below a path.
    /// </summary>
    string[] GetDirectories(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly);

    /// <summary>
    /// Determines whether a file exists.
    /// </summary>
    bool FileExists(string path);

    /// <summary>
    /// Determines whether a directory exists.
    /// </summary>
    bool DirectoryExists(string path);

    /// <summary>
    /// Creates a directory and any missing parent directories.
    /// </summary>
    void CreateDirectory(string path);

    /// <summary>
    /// Deletes a file.
    /// </summary>
    void DeleteFile(string path);

    /// <summary>
    /// Deletes a directory.
    /// </summary>
    void DeleteDirectory(string path, bool recursive);

    /// <summary>
    /// Copies a file.
    /// </summary>
    void CopyFile(string sourcePath, string destinationPath, bool overwrite);

    /// <summary>
    /// Moves a file.
    /// </summary>
    void MoveFile(string sourcePath, string destinationPath);

    /// <summary>
    /// Moves a directory.
    /// </summary>
    void MoveDirectory(string sourcePath, string destinationPath);

    /// <summary>
    /// Reads a text file asynchronously, one line at a time.
    /// </summary>
    IAsyncEnumerable<string> ReadLinesAsync(string path);

    /// <summary>
    /// Reads all bytes from a file, asynchronously
    /// </summary>
    Task<byte[]> ReadAllBytesAsync(string path);
}