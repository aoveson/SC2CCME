using System.Xml.Serialization;

namespace ModManager.StarCraft.Services.Log.IO;

/// <summary>
/// Represents the XML-serializable collection of log entries.
/// </summary>
[XmlType(TypeName = "LogCollection")]
public class LogCollection()
{
    /// <summary>Gets or sets the entries contained in the collection.</summary>
    [XmlArray]
    public List<LogEntry> LogEntries { get; set; } = [];
}