using ModManager.StarCraft.Base.Enum;
using ModManager.StarCraft.Services.Zip;
using ModManager.StarCraft.Services.Zip.Interface;

namespace ModManager.StarCraft.Base.UnitTest;

/// <summary>
/// Verifies custom campaign discovery, activation, restoration, and deletion.
/// </summary>
public sealed class CustomCampaignManagerUnitTest : IDisposable
{
    private readonly TestContext _context = new();
    private readonly IZipFileSystem _originalZipFileSystem = ZipService.FileSystem;

    /// <summary>Restores the process-wide ZIP seam after each test.</summary>
    public void Dispose()
    {
        ZipService.FileSystem = _originalZipFileSystem;
        _context.Dispose();
    }

    /// <summary>Loads metadata from campaign directories and ignores directories without metadata.</summary>
    [Fact]
    public async Task ModListsReadMetadataAndIgnoreDirectoriesWithoutMetadata()
    {
        InMemoryFileSystem fs = _context.FileSystem;
        fs.AddDirectory("sc2/Maps/CustomCampaigns/complete");
        fs.AddDirectory("sc2/Maps/CustomCampaigns/duplicate");
        fs.AddFile("sc2/Maps/CustomCampaigns/complete/metadata.txt", "title=Complete\ndesc=A description\ncampaign=wol\nversion=2\nauthor=Author\nignored line\n");
        fs.AddFile("sc2/Maps/CustomCampaigns/duplicate/metadata.txt", "title=Duplicate\ncampaign=hots\n");
        fs.AddFile("sc2/Maps/CustomCampaigns/duplicate/nested/metadata.txt", "title=Nested\n");
        fs.AddDirectory("sc2/Maps/CustomCampaigns/missing");

        List<ModList> result = await CustomCampaignManager.GetModListsAsync();

        Mod loaded = Assert.Single(result.Single(x => x.ModListCampaign == Campaign.WoL).Mods);
        Assert.Equal("Complete", loaded.Title);
        Assert.Equal("A description", loaded.Description);
        Assert.Equal("Author", loaded.Author);
        Assert.Equal("2", loaded.Version);
        Assert.Equal("sc2/Maps/CustomCampaigns/complete", loaded.Path);
        Assert.Equal("Duplicate", Assert.Single(result.Single(x => x.ModListCampaign == Campaign.HotS).Mods).Title);
        Assert.Empty(result.Single(x => x.ModListCampaign == Campaign.None).Mods);
    }

    /// <summary>Loads metadata for each campaign's active mod.</summary>
    [Fact]
    public async Task ActiveModsReadsEachCampaignMetadata()
    {
        foreach((Campaign campaign, string title) in new[]
        {
            (Campaign.WoL, "WoL"), (Campaign.HotS, "HotS"), (Campaign.LotV, "LotV"), (Campaign.NCO, "NCO")
        })
        {
            string path = CampaignPath(campaign);
            _context.FileSystem.AddFile($"{path}/metadata.txt", $"title={title}\ncampaign={title}\n");
        }

        List<Mod> activeMods = await CustomCampaignManager.GetActiveModsAsync();

        Assert.Equal(4, activeMods.Count);
        Assert.Equal([Campaign.WoL, Campaign.HotS, Campaign.LotV, Campaign.NCO], activeMods.Select(x => x.Campaign));
    }

    /// <summary>Copies, restores, and deletes campaign files through the filesystem seam.</summary>
    [Fact]
    public async Task SetRestoreAndDeleteModFilesManageCampaignDirectories()
    {
        InMemoryFileSystem fs = _context.FileSystem;
        fs.AddFile("source/nested/map.SC2Map", "map");
        fs.AddFile("sc2/Maps/Campaign/old.txt", "old");
        Mod mod = new() { Title = "Test Mod", Path = "source" };

        Assert.True(await CustomCampaignManager.SetActiveModAsync(Campaign.WoL, mod));
        Assert.Equal("map", fs.ReadText("sc2/Maps/Campaign/nested/map.SC2Map"));
        Assert.True(await CustomCampaignManager.RestoreToUnmodifiedAsync(Campaign.WoL));
        Assert.Empty(fs.GetFiles("sc2/Maps/Campaign"));

        fs.AddFile("sc2/Maps/CustomCampaigns/installed/map.SC2Map", "map");
        Assert.True(await CustomCampaignManager.DeleteModFilesAsync(new Mod { Title = "Installed", Path = "sc2/Maps/CustomCampaigns/installed" }));
        Assert.False(fs.DirectoryExists("sc2/Maps/CustomCampaigns/installed"));
    }

    /// <summary>Rejects deletion requests whose paths are outside CustomCampaigns.</summary>
    [Fact]
    public async Task DeleteModFilesRejectsPathsOutsideCustomCampaigns()
    {
        bool deleted = await CustomCampaignManager.DeleteModFilesAsync(new Mod
        {
            Title = "Outside",
            Path = "outside/mod"
        });

        Assert.False(deleted);
    }

    /// <summary>Preserves built-in campaign directories while removing custom files.</summary>
    [Fact]
    public async Task RestorePreservesBuiltInCampaignDirectories()
    {
        InMemoryFileSystem fs = _context.FileSystem;
        fs.AddDirectory("sc2/Maps/Campaign/swarm/evolution");
        fs.AddDirectory("sc2/Maps/Campaign/voidprologue");
        fs.AddFile("sc2/Maps/Campaign/custom/file.txt", "remove");

        Assert.True(await CustomCampaignManager.RestoreToUnmodifiedAsync(Campaign.WoL));
        Assert.True(fs.DirectoryExists("sc2/Maps/Campaign/swarm"));
        Assert.True(fs.DirectoryExists("sc2/Maps/Campaign/voidprologue"));
        Assert.False(fs.DirectoryExists("sc2/Maps/Campaign/custom"));
    }

    private string CampaignPath(Campaign campaign) => campaign switch
    {
        Campaign.WoL => "sc2/Maps/Campaign",
        Campaign.HotS => "sc2/Maps/Campaign/swarm",
        Campaign.LotV => "sc2/Maps/Campaign/void",
        Campaign.NCO => "sc2/Maps/Campaign/nova",
        _ => string.Empty
    };

}
