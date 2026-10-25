using Velopack;
using Velopack.Exceptions;
using Velopack.Sources;
using ModManager.StarCraft.Services.Log;
using ModManager.StarCraft.Services.Settings;
using ModManager.StarCraft.Services.Log.LogTypes;
using Velopack.Locators;
using ModManager.StarCraft.Services.Updater.Enum;
using Velopack.Logging;

namespace ModManager.StarCraft.Services.Updater;

/// <summary>
/// Represents a service that handles updates for the application, including checking for new versions and applying updates.
/// </summary>
public class VelopackUpdateService
{
    #region Fields
    /// <summary>
    /// The update source to use for fetching updates.
    /// </summary>
    private readonly IUpdateSource _updateSource;

    /// <summary>
    /// The update manager responsible for handling the update process, including checking for updates and applying them.
    /// This manager is initialized with a source that provides update information, such as a GitHub repository.
    /// </summary>
    private readonly UpdateManager _manager;

    /// <summary>
    /// Indicates whether updates are disabled for the application.
    /// This flag is used to determine whether the update process should be skipped.
    /// </summary>
    private readonly bool _updatesDisabled;
    #endregion

    #region Properties
    /// <summary>
    /// Indicates whether the application was recently updated.
    /// </summary>
    public bool RecentlyUpdated { get; private set; }
    #endregion

    #region Public Static Methods
    /// <summary>
    /// Initializes and runs the Velopack update service. 
    /// This method sets up the necessary components and starts the update process.
    /// This method should be called at the start of the application to ensure that 
    /// the update service is properly initialized.
    /// </summary>
    public static void InitializeVelopackUpdateService() =>
        VelopackApp.Build().Run();
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="VelopackUpdateService"/> class and updates the application version information if applicable.
    /// </summary>
    public VelopackUpdateService()
    {
        UpdateApplicationVersionInfoIfApplicable();

        UpdateSourceOption source = ApplicationSettings.QuerySettings(x => x.UpdateSource);
        IUpdateSource updateSource = null;
        switch (source)
        {
            case UpdateSourceOption.Github:
                string gitHubRepo = ApplicationSettings.QuerySettings(x => x.GithubUpdateRepo);
                bool usePreRelease = ApplicationSettings.QuerySettings(x => x.UsePreReleaseUpdates);
                updateSource = new GithubSource(gitHubRepo, null, usePreRelease);
                break;
            case UpdateSourceOption.Directory:
                string path = ApplicationSettings.QuerySettings(x => x.DirectoryUpdateSourcePath);
                DirectoryInfo di = new(path);
                if(!di.Exists)
                    di.Create();
                updateSource = new SimpleFileSource(new DirectoryInfo(path));
                break;
            case UpdateSourceOption.Web:
                string uri = ApplicationSettings.QuerySettings(x => x.WebUpdateSourceUri);
                updateSource = new SimpleWebSource(uri);
                break;
            case UpdateSourceOption.None:
                updateSource = null;
                _updatesDisabled = true;
                break;
        }

        if (updateSource != null)
        {
            _updateSource = updateSource;
            bool allowDowngrade = ApplicationSettings.QuerySettings(x => x.AllowDowngrade);
            _manager = new(updateSource, new UpdateOptions { AllowVersionDowngrade = allowDowngrade });
        }
        else if (!_updatesDisabled)
        {
            throw new InvalidOperationException("No valid update source configured. Please check your application settings.");
        }
    }
    #endregion

    #region Public Methods    
    /// <summary>
    /// Checks for available updates asynchronously.
    /// </summary>
    /// <param name="ct">The cancellation token to observe while waiting for the update check to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the update result if available, or null if no updates are found or an error occurs.</returns>
    public async Task<UpdateResult?> CheckForUpdatesAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        UpdateResult retVal = null;
        try
        {
            if(!_updatesDisabled)
            {
                UpdateInfo info = null;
                bool allowDowngrade = ApplicationSettings.QuerySettings(x => x.AllowDowngrade);
                string downgradeVersionTarget = ApplicationSettings.QuerySettings(x => x.DowngradeVersionTarget);
                if(allowDowngrade && !string.IsNullOrEmpty(downgradeVersionTarget))
                {
                    VelopackAssetFeed feed = await _updateSource.GetReleaseFeed(NullVelopackLogger.Instance, _manager.AppId, channel: null);
                    VelopackAsset target = feed.Assets
                        .Where(x => x.Type == VelopackAssetType.Full)
                        .Single(x => x.Version == SemanticVersion.Parse(downgradeVersionTarget));

                    bool isDowngrade = _manager.CurrentVersion != null && target.Version < _manager.CurrentVersion;
                    info = new UpdateInfo(target, isDowngrade);
                }
                else
                {
                    info = await _manager.CheckForUpdatesAsync();   
                }

                if (info != null)
                {
                    retVal = new UpdateResult
                    {
                        NewVersion = info.TargetFullRelease,
                        Info = info
                    };
                }
            }

            return retVal;
        }
        catch (NotInstalledException)
        {
            // The application is not installed, so we can ignore this exception.
            await Service.Get<Logger>().LogAsync<UpdaterDebug>([
                "Running in dev mode (not vpk-installed)."
            ]);
        }
        catch (Exception ex)
        {
            await Service.Get<Logger>().LogAsync<UpdaterError>([
                $"{nameof(CheckForUpdatesAsync)}",
                "An exception occurred while checking for updates.",
                ex.ToString()
            ]);
        }

        return retVal;
    }

    /// <summary>
    /// Applies the specified update and optionally restarts the application.
    /// </summary>
    /// <param name="result">The update result to apply.</param>
    public void ApplyUpdate(UpdateResult? result)
    {
        if (result != null && !_updatesDisabled)
        {
            ApplicationSettings.QuerySettings(x => x.PendingUpdate = true);
            try
            {
                _manager!.ApplyUpdatesAndRestart(result.Info);
            }
            catch
            {
                ApplicationSettings.QuerySettings(x => x.PendingUpdate = false);
                throw;
            }
        }
    }

    /// <summary>
    /// Updates the application version information if the currently installed version differs from the reported version in the application settings.
    /// Sets the RecentlyUpdated flag to true if an update is applied.
    /// </summary>
    private void UpdateApplicationVersionInfoIfApplicable()
    {
        string installedVersion = VelopackLocator.Current.CurrentlyInstalledVersion?.ToString();
        if(!string.IsNullOrWhiteSpace(installedVersion))
        {
            bool pendingUpdate = ApplicationSettings.QuerySettings(x => x.PendingUpdate);
            string reportedVersion = ApplicationSettings.QuerySettings(x => x.ApplicationVersion).TrimStart('v');
            if(pendingUpdate && installedVersion != reportedVersion)
            {
                ApplicationSettings.QuerySettings(x => x.ApplicationVersion = $"v{installedVersion}");
                ApplicationSettings.QuerySettings(x => x.PendingUpdate = false);
                RecentlyUpdated = true;
            }
        }
    }
    #endregion
}