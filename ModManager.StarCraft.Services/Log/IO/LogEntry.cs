using System.Xml.Serialization;
using ModManager.StarCraft.Services.Log.Enum;
using ModManager.StarCraft.Services.Log.Interface;

namespace ModManager.StarCraft.Services.Log.IO;

[XmlType(TypeName = "LogEntry")]
/// <summary>
/// Represents one structured application log entry.
/// </summary>
public class LogEntry
{
    /// <summary>Creates an empty log entry for XML deserialization.</summary>
    public LogEntry()
    {
    }

    /// <summary>Creates a log entry with its core metadata.</summary>
    /// <param name="logTitle">The entry title.</param>
    /// <param name="severity">The severity of the entry.</param>
    /// <param name="logMessage">The entry message.</param>
    /// <param name="logCode">The application-specific event code.</param>
    public LogEntry(string logTitle, LogSeverity severity, string logMessage, int logCode)
    {
        LogTitle = logTitle;
        Severity = severity;
        LogMessage = logMessage;
        LogCode = logCode;
    }

    #region Properties
    /// <summary>
    /// The timestamp of the log entry, indicating when the log was created.
    /// </summary>
    [XmlAttribute]
    public DateTime Timestamp { get; set; } = DateTime.Now;
    
    /// <summary>
    /// The title of the log entry, providing a brief description of the log's purpose or context.
    /// </summary>
    [XmlElement]
    public string LogTitle { get; set; } = string.Empty;

    /// <summary>
    /// The severity level of the log entry, indicating the importance or urgency of the log event.
    /// </summary>
    [XmlElement]
    public LogSeverity Severity { get; set; }

    /// <summary>
    /// The detailed message of the log entry, providing specific information about the log event.
    /// </summary>
    [XmlElement]
    public string LogMessage { get; set; } = string.Empty;

    /// <summary>
    /// The unique code associated with the log entry, which can be used for categorization or identification of specific log events.
    /// </summary>
    [XmlElement]
    public int LogCode { get; set; }

    /// <summary>
    /// An optional string containing additional information related to the log entry. 
    /// This can include extra context or parameters that provide further insight into the log event.
    /// </summary>
    [XmlElement]
    public string AdditionalInfo { get; set; } = string.Empty;

    /// <summary>Gets the approximate serialized size of the entry.</summary>
    [XmlIgnore]
    public int Size
    {
        get
        {
            int size = 0;
            size += Timestamp.ToString().Length;
            size += LogTitle.Length;
            size += Severity.ToString().Length;
            size += LogMessage.Length;
            size += sizeof(int); //Log code
            size += AdditionalInfo.Length;

            return size;
        }
    }
    #endregion

    #region Static Methods
    /// <summary>
    /// Creates a LogEntry from a given LogTypeBase instance and optional additional information.
    /// This method extracts the relevant properties from the LogTypeBase instance and constructs a LogEntry
    /// with the current timestamp, log title, severity, message, code, and any additional information provided.
    /// </summary>
    /// <param name="logType">The log type instance to initialize the entry with.</param>
    /// <param name="additionalInfo">Optional array of additional information to include in the log entry.</param>
    /// <returns>A new LogEntry instance initialized with the provided log type and additional information.</returns>
    public static LogEntry FromLogType(ILogType logType, string[]? additionalInfo = null)
    {
        LogEntry entry = new(logType.LogTitle, logType.Severity, logType.LogMessage, logType.LogCode);

        if (additionalInfo?.Length > 0)
        {
            List<string> additionalInfoParts = [];
            for(int i = 0 ; i < additionalInfo.Length; i++)
            {
                if(i < logType.AdditionalInfoHeaders.Length)
                {
                    additionalInfoParts.Add($"{logType.AdditionalInfoHeaders[i]}: {additionalInfo[i]}");
                }
                else
                {
                    additionalInfoParts.Add($"UNNAMED{i + 1}: {additionalInfo[i]}");
                }
            }

            entry.AdditionalInfo = string.Join(", ", additionalInfoParts);
        }

        return entry;
    }
    #endregion
}