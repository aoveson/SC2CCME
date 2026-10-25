using System.Xml.Serialization;
using ModManager.StarCraft.Services.Settings;

namespace ModManager.StarCraft.Services.Log.IO;

/// <summary>
/// Reads, writes, and trims XML log files.
/// </summary>
public static class LogReaderWriter
{
    /// <summary>
    /// Writes a log entry to the specified destination file path asynchronously.
    /// </summary>
    /// <param name="collection">The log collection to write.</param>
    /// <param name="path">The path to the destination file.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task WriteLogAsync(LogCollection collection, string path)
    {
        await WriteCollectionAsync(collection, path);
        await TrimLogAsync(collection, path);
    }

    private static async Task WriteCollectionAsync(LogCollection collection, string path)
    {
        XmlSerializer serializer = new(typeof(LogCollection));
        using StreamWriter writer = new(path);
        serializer.Serialize(writer, collection);
        await writer.FlushAsync();
    }

    /// <summary>
    /// Reads a log collection from an XML file, recovering with a new collection when deserialization fails.
    /// </summary>
    /// <param name="path">The path to the log file.</param>
    /// <returns>The deserialized collection, or an empty collection after recovery.</returns>
    public static LogCollection ReadLog(string path)
    {
        try
        {
            LogCollection retVal = new();
            XmlSerializer serializer = new(typeof(LogCollection));
            using StreamReader reader = new(path);
            if(serializer.Deserialize(reader) is LogCollection lc)
            {
                retVal = lc;
            }

            return retVal;
        }
        catch(Exception)
        {
            // Could not read the log, the log file is corrupted, 
            // lets move it to a backup and make a new one
            DateTime now = DateTime.Now;
            string moveTime = $"{now.Month}_{now.Day}_{now.Year}_{now.Hour}{now.Minute}{now.Second}";
            string newFileName = path.Replace(".xml", $".old.{moveTime}.xml");
            File.Move(path, newFileName);

            return new LogCollection();
        }
    }

    /// <summary>
    /// Trims the log file to the maximum bytes limited by the <c>FileLoggingMaxMegabytes</c> setting.
    /// </summary>
    /// <param name="collection">The collection whose oldest entries may be removed.</param>
    /// <param name="path">The path to the log file</param>
    /// <returns>A task representing the asynchronous trim operation.</returns>
    public static async Task TrimLogAsync(LogCollection collection, string path)
    {
        long maxBytes = ApplicationSettings.QuerySettings(x => x.FileLoggingMaxMegabytes) * 1024L * 1024L;
        if (File.Exists(path))
        {
            byte[] logBytes = await File.ReadAllBytesAsync(path);
            if (logBytes.LongLength >= maxBytes)
            {
                long bytesToRemove = logBytes.LongLength - maxBytes;
                int bytesRemoved = 0;
                while(bytesRemoved < bytesToRemove && collection.LogEntries.Count > 0)
                {
                    int entrySize = collection.LogEntries[0].Size;
                    collection.LogEntries.RemoveAt(0);
                    bytesRemoved += entrySize;
                }

                await WriteCollectionAsync(collection, path);
            }
        }
    }
}