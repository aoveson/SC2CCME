using ModManager.StarCraft.Services.Log.Enum;
using ModManager.StarCraft.Services.Log.Interface;

namespace ModManager.StarCraft.Services.Log.LogTypes;

/// <summary>Describes an error raised during an update operation.</summary>
public class UpdaterError : ILogType
{
    /// <inheritdoc />
    public string LogTitle => "Updater Error";

    /// <inheritdoc />
    public LogSeverity Severity => LogSeverity.Error;

    /// <inheritdoc />
    public string LogMessage => "An error occurred during the update process.";

    /// <inheritdoc />
    public string[] AdditionalInfoHeaders => ["Stage", "ErrorMessage", "Exception"];

    /// <inheritdoc />
    public int LogCode => 1004;
}