using ModManager.StarCraft.Base.Enum;

namespace ModManager.StarCraft.Base;

/// <summary>
/// Groups installed mods by campaign.
/// </summary>
public class ModList(Campaign campaign)
{
    /// <summary>Gets the campaign represented by this list.</summary>
    public Campaign ModListCampaign { get; } = campaign;

    /// <summary>Gets the mods belonging to the campaign.</summary>
    public List<Mod> Mods { get; } = [];
}