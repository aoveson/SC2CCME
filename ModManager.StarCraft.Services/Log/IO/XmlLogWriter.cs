using ModManager.StarCraft.Services.Log.Interface;

namespace ModManager.StarCraft.Services.Log.IO;

/// <summary>
/// Persists logs through the XML log reader and writer.
/// </summary>
public sealed class XmlLogWriter : ILogWriter
{
    /// <inheritdoc />
    public LogCollection Read(string path) => LogReaderWriter.ReadLog(path);

    /// <inheritdoc />
    public Task WriteAsync(LogCollection collection, string path) => LogReaderWriter.WriteLogAsync(collection, path);
}