using ModManager.StarCraft.Services.Updater;

namespace ModManager.StarCraft.Base.Package;

/// <summary>
/// Contains the result of checking for an application update.
/// </summary>
public class CheckForUpdatesResult(bool updateAvailable, UpdateResult updateResult)
{
    /// <summary>Gets whether an update is available.</summary>
    public bool UpdateAvailable { get; } = updateAvailable;
    /// <summary>Gets the available update, when one was found.</summary>
    public UpdateResult Update { get; } = updateResult;
}