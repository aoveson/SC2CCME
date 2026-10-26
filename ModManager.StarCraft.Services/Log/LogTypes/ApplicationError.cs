using ModManager.StarCraft.Services.Log.Enum;
using ModManager.StarCraft.Services.Log.Interface;

namespace ModManager.StarCraft.Services.Log.LogTypes;

/// <summary>
/// Represents an error log entry for application-level error information.
/// </summary>
public class ApplicationError : ILogType
{
    /// <inheritdoc />
    public string LogTitle => "Application Error";

    /// <inheritdoc />
    public LogSeverity Severity => LogSeverity.Error;

    /// <inheritdoc />
    public string LogMessage => "An application error has occurred.";

    /// <inheritdoc />
    public string[] AdditionalInfoHeaders => ["Error"];

    /// <inheritdoc />
    public int LogCode => 1007;
}