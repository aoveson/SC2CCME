using ModManager.StarCraft.Services.Log.Enum;
using ModManager.StarCraft.Services.Log.Interface;

namespace ModManager.StarCraft.Services.Log.LogTypes;

/// <summary>Describes an informational application startup event.</summary>
public class StartupInfo : ILogType
{
    /// <inheritdoc />
    public string LogTitle => "Startup Info";

    /// <inheritdoc />
    public LogSeverity Severity => LogSeverity.Info;

    /// <inheritdoc />
    public string LogMessage => "Startup info message.";

    /// <inheritdoc />
    public string[] AdditionalInfoHeaders => ["Info"];

    /// <inheritdoc />
    public int LogCode => 1012;
}