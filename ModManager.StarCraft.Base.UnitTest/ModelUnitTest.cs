using ModManager.StarCraft.Base.Enum;
using ModManager.StarCraft.Base.Package;

namespace ModManager.StarCraft.Base.UnitTest;

/// <summary>
/// Verifies the small domain models and result packages used by the Base project.
/// </summary>
public sealed class ModelUnitTest
{
    /// <summary>Maps campaign metadata and formats a mod title.</summary>
    [Fact]
    public void ModMapsCampaignNamesAndFormatsItsTitle()
    {
        Mod mod = new() { Title = "Campaign", Version = "1.2" };

        mod.SetCampaign("Wings of Liberty");
        Assert.Equal(Campaign.WoL, mod.Campaign);
        mod.SetCampaign("heart of the swarm");
        Assert.Equal(Campaign.HotS, mod.Campaign);
        mod.SetCampaign("Legacy of the Void");
        Assert.Equal(Campaign.LotV, mod.Campaign);
        mod.SetCampaign("Nova covert ops");
        Assert.Equal(Campaign.NCO, mod.Campaign);
        mod.SetCampaign("unrecognized");
        Assert.Equal(Campaign.None, mod.Campaign);
        Assert.Equal("Campaign 1.2", mod.ToString());
    }

    /// <summary>Exposes constructor values from campaign lists and result packages.</summary>
    [Fact]
    public void ModListAndResultPackagesExposeTheirValues()
    {
        ModList modList = new(Campaign.WoL);
        Mod mod = new() { Title = "Test" };
        modList.Mods.Add(mod);
        List<ModList> modLists = [modList];
        List<Mod> activeMods = [mod];
        CheckForUpdatesResult updateResult = new(false, null!);
        ImportCampaignZipsResult importResult = new(modLists, activeMods, mod);

        Assert.Equal(Campaign.WoL, modList.ModListCampaign);
        Assert.Same(mod, Assert.Single(modList.Mods));
        Assert.False(updateResult.UpdateAvailable);
        Assert.Null(updateResult.Update);
        Assert.Same(modLists, importResult.ModLists);
        Assert.Same(activeMods, importResult.ActiveMods);
        Assert.Same(mod, importResult.ModAvailableToActivate);
    }
}
