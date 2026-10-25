using ModManager.StarCraft.Services.Log.Enum;
using ModManager.StarCraft.Services.Log.Interface;

namespace ModManager.StarCraft.Services.Log.LogTypes;

/// <summary>Describes an informational file or directory operation.</summary>
public class IOInfo : ILogType
{
    /// <inheritdoc />
    public string LogTitle => "I/O Info";

    /// <inheritdoc />
    public LogSeverity Severity => LogSeverity.Info;

    /// <inheritdoc />
    public string LogMessage => "An I/O information event occurred.";

    /// <inheritdoc />
    public string[] AdditionalInfoHeaders => ["Message"];

    /// <inheritdoc />
    public int LogCode => 1009;
}