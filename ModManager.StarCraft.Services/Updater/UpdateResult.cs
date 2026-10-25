using Velopack;

namespace ModManager.StarCraft.Services.Updater;

/// <summary>
/// Represents the result of an update check, including the new version available and any release notes.
/// </summary>
public record UpdateResult
{
    /// <summary>
    /// Gets the new version available for update.
    /// </summary>
    public required VelopackAsset NewVersion { get; init; }

    /// <summary>
    /// Gets the Velopack update information used to apply the update.
    /// </summary>
    internal UpdateInfo Info { get; init; } = null!;
}