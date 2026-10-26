using ModManager.StarCraft.Services.Log.Enum;
using ModManager.StarCraft.Services.Log.Interface;

namespace ModManager.StarCraft.Services.Log.LogTypes;

/// <summary>
/// Represents an info log entry for application-level informational messages.
/// </summary>
public class ApplicationInfo : ILogType
{
    /// <inheritdoc />
    public string LogTitle => "Application Info";

    /// <inheritdoc />
    public LogSeverity Severity => LogSeverity.Info;

    /// <inheritdoc />
    public string LogMessage => "Application info message.";

    /// <inheritdoc />
    public string[] AdditionalInfoHeaders => ["Info"];

    /// <inheritdoc />
    public int LogCode => 1010;
}