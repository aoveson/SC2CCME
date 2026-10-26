using ModManager.StarCraft.Services.Log.Enum;
using ModManager.StarCraft.Services.Log.Interface;

namespace ModManager.StarCraft.Services.Log.LogTypes;

/// <summary>Describes a ZIP archive extraction operation.</summary>
public class ZipInfo : ILogType
{
    /// <inheritdoc />
    public string LogTitle => "Zip Info";

    /// <inheritdoc />
    public LogSeverity Severity => LogSeverity.Info;

    /// <inheritdoc />
    public string LogMessage => "A zip file has been unzipped.";

    /// <inheritdoc />
    public string[] AdditionalInfoHeaders => ["ZipFilePath", "ExtractedToPath"];

    /// <inheritdoc />
    public int LogCode => 1006;
}