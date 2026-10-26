using ModManager.StarCraft.Services.Log.Enum;
using ModManager.StarCraft.Services.Log.Interface;

namespace ModManager.StarCraft.Services.Log.LogTypes;

/// <summary>Describes diagnostic information from the updater.</summary>
public class UpdaterDebug : ILogType
{
    /// <inheritdoc />
    public string LogTitle => "Updater Debug";

    /// <inheritdoc />
    public LogSeverity Severity => LogSeverity.Debug;

    /// <inheritdoc />
    public string LogMessage => "Debug information for the updater.";

    /// <inheritdoc />
    public string[] AdditionalInfoHeaders => ["DebugMessage"];

    /// <inheritdoc />
    public int LogCode => 1005;
}