namespace ModManager.StarCraft.Base.Package;

/// <summary>
/// Contains the campaign state produced by importing campaign ZIP archives.
/// </summary>
public class ImportCampaignZipsResult(List<ModList> modLists, List<Mod> activeMods, Mod? modAvailableToActivate)
{
    /// <summary>Gets the discovered campaign mod lists.</summary>
    public List<ModList> ModLists { get; } = modLists;

    /// <summary>Gets the currently active campaign mods.</summary>
    public List<Mod> ActiveMods { get; } = activeMods;

    /// <summary>Gets the imported mod that can be activated, if uniquely identified.</summary>
    public Mod? ModAvailableToActivate { get; } = modAvailableToActivate;
}