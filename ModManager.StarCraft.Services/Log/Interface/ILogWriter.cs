using ModManager.StarCraft.Services.Log.IO;

namespace ModManager.StarCraft.Services.Log.Interface;

/// <summary>
/// Provides persistence for log collections.
/// </summary>
public interface ILogWriter
{
    /// <summary>
    /// Reads a log collection from a path.
    /// </summary>
    /// <param name="path">The log path.</param>
    /// <returns>The loaded log collection.</returns>
    LogCollection Read(string path);

    /// <summary>
    /// Writes a log collection to a path.
    /// </summary>
    /// <param name="collection">The log collection.</param>
    /// <param name="path">The log path.</param>
    /// <returns>A task representing the write operation.</returns>
    Task WriteAsync(LogCollection collection, string path);
}