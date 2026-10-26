using Velopack.Locators;

namespace ModManager.StarCraft.Services.UnitTest;

/// <summary>Verifies the Velopack test locator used for non-installed update scenarios.</summary>
public sealed class VelopackLocatorUnitTest
{
    /// <summary>Describes a test application without requiring a real installation.</summary>
    [Fact]
    public void TestLocatorDescribesANonInstalledApplication()
    {
        TestVelopackLocator locator = new("SC2CCME-Test", "1.0.0", "packages");

        Assert.Equal("SC2CCME-Test", locator.AppId);
        Assert.Equal("1.0.0", locator.CurrentlyInstalledVersion?.ToString());
        Assert.Equal("packages", locator.PackagesDir);
        Assert.False(locator.IsPortable);
    }
}