using ModManager.StarCraft.Application.ViewModels;
using ModManager.StarCraft.Base;
using ModManager.StarCraft.Base.Enum;
using System.Reflection;

namespace ModManager.StarCraft.Application.UnitTest;

/// <summary>
/// Verifies the initial state and campaign presentation behavior of <see cref="MainViewModel"/>.
/// </summary>
public sealed class MainViewModelUnitTest
{
    /// <summary>
    /// Ensures construction wires every command and exposes the expected default campaign state.
    /// </summary>
    [Fact]
    public void ConstructorInitializesCommandsAndDefaultCampaignPresentation()
    {
        MainViewModel viewModel = new();

        Assert.NotNull(viewModel.WindowLoaded);
        Assert.NotNull(viewModel.SelectSc2);
        Assert.NotNull(viewModel.SelectZipToImport);
        Assert.NotNull(viewModel.CampaignZipDragOver);
        Assert.NotNull(viewModel.CampaignZipDrop);
        Assert.NotNull(viewModel.WingsOfLibertySetActiveCampaign);
        Assert.NotNull(viewModel.HeartOfTheSwarmRestoreToUnmodified);
        Assert.NotNull(viewModel.LegacyOfTheVoidDeleteModFiles);
        Assert.NotNull(viewModel.NovaCovertOpsSetActiveCampaign);

        Assert.Equal("Default Campaign", viewModel.WingsOfLibertyActiveModTitle);
        Assert.Equal("Blizzard", viewModel.HeartOfTheSwarmActiveModAuthor);
        Assert.Equal("N/A", viewModel.LegacyOfTheVoidActiveModVersion);
        Assert.False(viewModel.NovaCovertOpsButtonsEnabled);
        Assert.Empty(viewModel.WingsOfLibertyModList);
        Assert.Empty(viewModel.NovaCovertOpsModList);
    }

    /// <summary>
    /// Ensures campaign action availability follows the selected-mod and active-mod state.
    /// </summary>
    [Fact]
    public void SelectedModStateControlsCampaignActions()
    {
        MainViewModel viewModel = new();

        Assert.False(viewModel.WingsOfLibertyButtonsEnabled);
        Assert.False(viewModel.HeartOfTheSwarmButtonsEnabled);
        Assert.False(viewModel.LegacyOfTheVoidButtonsEnabled);

        viewModel.WingsOfLibertyModListSelectedItem = null!;
        viewModel.HeartOfTheSwarmModListSelectedItem = null!;
        viewModel.LegacyOfTheVoidModListSelectedItem = null!;

        Assert.False(viewModel.WingsOfLibertyButtonsEnabled);
        Assert.False(viewModel.HeartOfTheSwarmButtonsEnabled);
        Assert.False(viewModel.LegacyOfTheVoidButtonsEnabled);

        SetActiveMod(viewModel, "_novaCovertOpsActiveMod", new Mod { Title = "Nova", Campaign = Campaign.NCO });
        Assert.True(viewModel.NovaCovertOpsButtonsEnabled);
        Assert.Equal("Nova", viewModel.NovaCovertOpsActiveModTitle);
        Assert.Equal("N/A", viewModel.NovaCovertOpsActiveModVersion);
    }

    /// <summary>
    /// Ensures loaded active-mod details are reflected by the campaign display properties.
    /// </summary>
    [Fact]
    public void ActiveCampaignPropertiesExposeLoadedModDetails()
    {
        MainViewModel viewModel = new();
        SetActiveMod(viewModel, "_wingsOfLibertyActiveMod", new Mod
        {
            Title = "Wings",
            Author = "Author",
            Version = "1.0",
            Campaign = Campaign.WoL
        });
        viewModel.WingsOfLibertyModListSelectedItem = viewModel.WingsOfLibertyActiveMod;

        Assert.Equal("Wings", viewModel.WingsOfLibertyActiveModTitle);
        Assert.Equal("Author", viewModel.WingsOfLibertyActiveModAuthor);
        Assert.Equal("1.0", viewModel.WingsOfLibertyActiveModVersion);
        Assert.True(viewModel.WingsOfLibertyButtonsEnabled);
    }

    /// <summary>
    /// Seeds state normally populated by the private startup event handlers.
    /// </summary>
    private static void SetActiveMod(MainViewModel viewModel, string fieldName, Mod value) =>
        typeof(MainViewModel).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(viewModel, value);
}