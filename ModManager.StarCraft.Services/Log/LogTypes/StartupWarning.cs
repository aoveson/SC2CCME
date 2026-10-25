using ModManager.StarCraft.Services.Log.Enum;
using ModManager.StarCraft.Services.Log.Interface;

namespace ModManager.StarCraft.Services.Log.LogTypes;

/// <summary>Describes a warning raised during application startup.</summary>
public class StartupWarning : ILogType
{
    /// <inheritdoc />
    public string LogTitle => "Startup Warning";

    /// <inheritdoc />
    public LogSeverity Severity => LogSeverity.Warning;

    /// <inheritdoc />
    public string LogMessage => "A warning occurred during startup.";

    /// <inheritdoc />
    public string[] AdditionalInfoHeaders => ["WarningMessage"];

    /// <inheritdoc />
    public int LogCode => 1009;
}