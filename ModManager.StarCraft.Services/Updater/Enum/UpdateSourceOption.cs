namespace ModManager.StarCraft.Services.Updater.Enum;

/// <summary>
/// Identifies the source from which application updates are obtained.
/// </summary>
public enum UpdateSourceOption
{
    /// <summary>
    /// Represents an update source available from GitHub.
    /// </summary>
    Github,

    /// <summary>
    /// Represents an update source available from a local directory.
    /// </summary>
    Directory,

    /// <summary>
    /// Represents an update source available via the web.
    /// </summary>
    Web,

    /// <summary>
    /// Represents no update source configured.
    /// This is a valid setting when no updates are desired or configured.
    /// </summary>
    None
}