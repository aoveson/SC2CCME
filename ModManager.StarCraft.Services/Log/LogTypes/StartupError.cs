using ModManager.StarCraft.Services.Log.Enum;
using ModManager.StarCraft.Services.Log.Interface;

namespace ModManager.StarCraft.Services.Log.LogTypes;

/// <summary>Describes an error raised during application startup.</summary>
public class StartupError : ILogType
{
    /// <inheritdoc />
    public string LogTitle => "Startup Error";

    /// <inheritdoc />
    public LogSeverity Severity => LogSeverity.Error;

    /// <inheritdoc />
    public string LogMessage => "An error occurred during startup.";

    /// <inheritdoc />
    public string[] AdditionalInfoHeaders => ["ErrorMessage"];

    /// <inheritdoc />
    public int LogCode => 1008;
}