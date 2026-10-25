namespace ModManager.StarCraft.Services.Log.Enum;

/// <summary>
/// Defines the severity levels for log entries.
/// </summary>
public enum LogSeverity
{
    /// <summary>
    /// Debug level information, for development and troubleshooting purposes.
    /// </summary>
    Debug,

    /// <summary>
    /// Informational messages that highlight the progress of the application at a coarse-grained level.
    /// </summary>
    Info,

    /// <summary>
    /// Potentially harmful situations that still allow the application to continue running.
    /// </summary>
    Warning,

    /// <summary>
    /// Error events that indicate a failure in the application or a process.
    /// </summary>
    Error
}