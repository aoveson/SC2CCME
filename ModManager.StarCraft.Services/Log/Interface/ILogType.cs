using ModManager.StarCraft.Services.Log.Enum;

namespace ModManager.StarCraft.Services.Log.Interface;

/// <summary>
/// Defines the structure for log types, ensuring that each log entry has a title, severity level, message, code, and 
/// optional additional information headers.
/// </summary>
public interface ILogType
{
    /// <summary>
    /// The title of the log entry, providing an easily understandable description of the log event.
    /// </summary>
    public abstract string LogTitle { get; }

    /// <summary>
    /// The severity level of the log entry, indicating the importance or urgency of the log event.
    /// </summary>
    public abstract LogSeverity Severity { get; }

    /// <summary>
    /// The detailed message of the log entry, providing specific information about the log event.
    /// </summary>
    public abstract string LogMessage { get; }

    /// <summary>
    /// The unique code associated with the log entry, which can be used for categorization or identification of specific log events.
    /// </summary>
    public abstract int LogCode { get; }

    /// <summary>
    /// An array of additional parameter headers that can be used to provide extra context or information for the log entry. 
    /// This can be useful for including supplementary data that may not fit into the standard log message format.
    /// </summary>
    public abstract string[] AdditionalInfoHeaders { get; }
}