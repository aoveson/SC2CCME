using Velopack;
using Velopack.Exceptions;
using Velopack.Sources;
using ModManager.StarCraft.Services.Log;
using ModManager.StarCraft.Services.Settings;
using ModManager.StarCraft.Services.Log.LogTypes;
using Velopack.Locators;
using ModManager.StarCraft.Services.Updater.Enum;
using Velopack.Logging;
using System.Runtime.InteropServices;
using System.Text;
using ModManager.StarCraft.Utilities;
using ModManager.StarCraft.Utilities.Interface;

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

    public IFileSystem FileSystem { get; set; } = new PhysicalFileSystem();
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
    public async Task ApplyUpdateAsync(UpdateResult? result)
    {
        if (result != null && !_updatesDisabled)
        {
            try
            {
                _manager!.ApplyUpdatesAndRestart(result.Info);
            }
            catch (Exception e)
            {
                var diagnosticInfo = new StringBuilder();
                diagnosticInfo.AppendLine($"=== SC2CCME Update Failure Report ===");
                diagnosticInfo.AppendLine($"Timestamp: {DateTime.UtcNow:O}");
                diagnosticInfo.AppendLine($"Current Version: {ApplicationSettings.QuerySettings(x => x.ApplicationVersion)}");
                diagnosticInfo.AppendLine($"Target Version: {result.NewVersion.Version}");
                diagnosticInfo.AppendLine($"OS: {Environment.OSVersion} ({RuntimeInformation.OSArchitecture})");
                diagnosticInfo.AppendLine($"Process Architecture: {RuntimeInformation.ProcessArchitecture}");
                diagnosticInfo.AppendLine($".NET Runtime: {Environment.Version}");
                diagnosticInfo.AppendLine($"Base Directory: {AppContext.BaseDirectory}");
                diagnosticInfo.AppendLine();
                diagnosticInfo.AppendLine("--- Exception Details ---");
                diagnosticInfo.AppendLine();
                diagnosticInfo.AppendLine(e.ToString());

                try
                {
                    string dumpFileName = $"update_failure_{DateTime.Now:yyyyMMdd-HHmmss}.log";
                    string dumpPath = Path.Combine(ApplicationDataHelper.GetCCMDataPathCreateIfNotExists(), dumpFileName);
                    await FileSystem.WriteAllTextAsync(Path.Combine(dumpPath, $"update-crash-{DateTime.Now:yyyyMMdd-HHmmss}.log"), diagnosticInfo.ToString());
                }
                catch {} // dump failed for some reason, fail out and just return the exception
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
            string reportedVersion = ApplicationSettings.QuerySettings(x => x.ApplicationVersion).TrimStart('v');
            if(installedVersion != reportedVersion)
            {
                ApplicationSettings.QuerySettings(x => x.ApplicationVersion = $"v{installedVersion}");
                RecentlyUpdated = true;
            }
        }
    }
    #endregion
}