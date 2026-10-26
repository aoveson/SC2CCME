using ModManager.StarCraft.Base.Enum;

namespace ModManager.StarCraft.Base;

/// <summary>
/// Core metadata for a mod.
/// </summary>
public class Mod
{
    /// <summary>
    /// Gets or sets the title of the mod.
    /// </summary>
    public string Title { get; set; } = "N/A";

    /// <summary>
    /// Gets or sets the author of the mod.
    /// </summary>
    public string Author { get; set; } = "N/A";

    /// <summary>
    /// Gets or sets the description of the mod.
    /// </summary>
    public string Description { get; set; } = "N/A";

    /// <summary>
    /// Gets or sets the campaign to which the mod belongs.
    /// </summary>
    public Campaign Campaign { get; set; } = Campaign.None;

    /// <summary>
    /// Gets or sets the path of the mod.
    /// </summary>
    public string Path { get; set; } = "";

    /// <summary>
    /// Gets or sets the version of the mod.
    /// </summary>
    public string Version { get; set; } = "N/A";

    /// <summary>
    /// Sets the campaign by matching a campaign name or abbreviation in the supplied value.
    /// </summary>
    /// <param name="_campaign">The campaign name or abbreviation to match.</param>
    public void SetCampaign(string _campaign)
    {
        _campaign = _campaign.ToLower();
        if (_campaign.Contains("wings") || _campaign.Contains("liberty") || _campaign.Contains("wol"))
        {
            Campaign = Campaign.WoL;
            return;
        }
        if (_campaign.Contains("heart") || _campaign.Contains("swarm") || _campaign.Contains("hots"))
        {
            Campaign = Campaign.HotS;
            return;
        }
        if (_campaign.Contains("legacy") || _campaign.Contains("void") || _campaign.Contains("lotv"))
        {
            Campaign = Campaign.LotV;
            return;
        }
        if (_campaign.Contains("nova") || _campaign.Contains("covert") || _campaign.Contains("ops") || _campaign.Contains("nco"))
        {
            Campaign = Campaign.NCO;
            return;
        }
        Campaign = Campaign.None;
    }

    /// <inheritdoc />
    public override string ToString() => $"{Title} {Version}";
}