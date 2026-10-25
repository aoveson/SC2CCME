using ModManager.StarCraft.Services.Log.Enum;
using ModManager.StarCraft.Services.Log.Interface;

namespace ModManager.StarCraft.Services.Log.LogTypes;

/// <summary>
/// Represents a debug log entry for application-level debugging information.
/// </summary>
public class ApplicationDebug : ILogType
{
    /// <inheritdoc />
    public string LogTitle => "Application Debug";

    /// <inheritdoc />
    public LogSeverity Severity => LogSeverity.Debug;

    /// <inheritdoc />
    public string LogMessage => "Application debug message.";

    /// <inheritdoc />
    public string[] AdditionalInfoHeaders => ["DebugInfo"];

    /// <inheritdoc />
    public int LogCode => 1001;
}