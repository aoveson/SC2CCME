using ModManager.StarCraft.Services.Log.Enum;
using ModManager.StarCraft.Services.Log.Interface;

namespace ModManager.StarCraft.Services.Log.LogTypes;

/// <summary>Describes an error during a file or directory operation.</summary>
public class IOError : ILogType
{
    /// <inheritdoc />
    public string LogTitle => "I/O Error";

    /// <inheritdoc />
    public LogSeverity Severity => LogSeverity.Error;

    /// <inheritdoc />
    public string LogMessage => "An I/O error occurred during file or directory operations.";

    /// <inheritdoc />
    public string[] AdditionalInfoHeaders => ["ErrorMessage", "Exception"];

    /// <inheritdoc />
    public int LogCode => 1003;
}