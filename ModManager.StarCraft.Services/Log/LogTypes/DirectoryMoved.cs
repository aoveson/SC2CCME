using ModManager.StarCraft.Services.Log.Enum;
using ModManager.StarCraft.Services.Log.Interface;

namespace ModManager.StarCraft.Services.Log.LogTypes;

/// <summary>Describes a directory move or rename operation.</summary>
public class DirectoryMoved : ILogType
{
    /// <inheritdoc />
    public string LogTitle => "Directory Moved";

    /// <inheritdoc />
    public LogSeverity Severity => LogSeverity.Info;

    /// <inheritdoc />
    public string LogMessage => "A directory has been moved or renamed.";

    /// <inheritdoc />
    public string[] AdditionalInfoHeaders => ["SourcePath", "DestinationPath", "Reason"];

    /// <inheritdoc />
    public int LogCode => 1002;
}