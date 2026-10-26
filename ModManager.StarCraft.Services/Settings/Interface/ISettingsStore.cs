namespace ModManager.StarCraft.Services.Settings.Interface;

/// <summary>
/// Provides persistence for application settings.
/// </summary>
public interface ISettingsStore
{
    /// <summary>
    /// Reads the current settings.
    /// </summary>
    /// <returns>The persisted settings.</returns>
    ApplicationSettings Read();

    /// <summary>
    /// Persists the supplied settings.
    /// </summary>
    /// <param name="settings">The settings to persist.</param>
    void Write(ApplicationSettings settings);
}