using ModManager.StarCraft.Services.Log.Enum;
using ModManager.StarCraft.Services.Log.Interface;

namespace ModManager.StarCraft.Services.Log.LogTypes;

/// <summary>Describes an informational campaign manager event.</summary>
public class CampaignManagerInfo : ILogType
{
    /// <inheritdoc />
    public string LogTitle => "Campaign Manager Info";

    /// <inheritdoc />
    public LogSeverity Severity => LogSeverity.Info;

    /// <inheritdoc />
    public string LogMessage => "A campaign manager info event occurred.";

    /// <inheritdoc />
    public string[] AdditionalInfoHeaders => ["InfoMessage"];

    /// <inheritdoc />
    public int LogCode => 1012;
}