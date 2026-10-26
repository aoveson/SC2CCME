using ModManager.StarCraft.Services.Log.Enum;
using ModManager.StarCraft.Services.Log.Interface;

namespace ModManager.StarCraft.Services.Log.LogTypes;

/// <summary>Describes an error raised during a campaign manager operation.</summary>
public class CampaignManagerError : ILogType
{
    /// <inheritdoc />
    public string LogTitle => "Campaign Manager Error";

    /// <inheritdoc />
    public LogSeverity Severity => LogSeverity.Error;

    /// <inheritdoc />
    public string LogMessage => "An error occurred during a campaign operation.";

    /// <inheritdoc />
    public string[] AdditionalInfoHeaders => ["ErrorMessage", "Exception"];

    /// <inheritdoc />
    public int LogCode => 1011;
}