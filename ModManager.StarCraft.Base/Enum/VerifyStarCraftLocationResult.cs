namespace ModManager.StarCraft.Base.Enum;

/// <summary>
/// Describes whether the configured StarCraft II executable can be found.
/// </summary>
public enum VerifyStarCraftLocationResult
{
    /// <summary>
    /// Thrown if "StarCraft II.exe" does not exist where settings indicates it should.
    /// </summary>
    DoesNotExist,

    /// <summary>
    /// "StarCraft II.exe" exists in the location identified in the settings file.
    /// </summary>
    Success
}