using ModManager.StarCraft.Base.Enum;
using ModManager.StarCraft.Base.Package;
using ModManager.StarCraft.Services;
using ModManager.StarCraft.Services.Log;
using ModManager.StarCraft.Services.Log.Enum;
using ModManager.StarCraft.Services.Log.LogTypes;
using ModManager.StarCraft.Services.Updater;
using ModManager.StarCraft.Services.Settings;
using ModManager.StarCraft.Services.Zip;
using ModManager.StarCraft.Utilities;
using ModManager.StarCraft.Utilities.Interface;

namespace ModManager.StarCraft.Base;

/// <summary>
/// Coordinates application startup tasks and publishes their progress.
/// </summary>
public class Startup
{
    /// <summary>
    /// Gets or sets the filesystem used by startup operations.
    /// </summary>
    public static IFileSystem FileSystem { get; set; } = new PhysicalFileSystem();

    #region Fields
    private int _startupStep = 0;
    #endregion
    #region Properties

    /// <summary>Gets the application version loaded during startup creation.</summary>
    public string AppVersion {get; private set;}

    /// <summary>Gets the platform detected during startup creation.</summary>
    public ExecutingPlatform Platform {get; private set;}

    private List<Func<Task>> StartupSteps {get; set;}
    #endregion

    #region Constructor
    private Startup() {}
    #endregion

    #region Events
    /// <summary>Raised after the update check completes.</summary>
    public event EventHandler<CheckForUpdatesResult> CheckForUpdatesCompleted;

    /// <summary>Raised after the StarCraft II location is verified.</summary>
    public event EventHandler<VerifyStarCraftLocationResult> VerifyStarCraftLocationCompleted;

    /// <summary>Raised after active campaign mods are loaded.</summary>
    public event EventHandler<List<Mod>> GetActiveModsCompleted;

    /// <summary>Raised after installed campaign lists are loaded.</summary>
    public event EventHandler<List<ModList>> GetModListsCompleted;

    /// <summary>Raised after all startup steps complete.</summary>
    public event EventHandler StartupCompleted;
    #endregion

    #region Initialization
    /// <summary>
    /// Creates a startup coordinator and configures its ordered workflow.
    /// </summary>
    /// <returns>A configured startup coordinator.</returns>
    public static Startup Create()
    {
        Startup startup = new()
        {
            Platform = OperatingSystem.IsWindows() ? ExecutingPlatform.Windows :
                OperatingSystem.IsMacOS() ? ExecutingPlatform.MacOS :
                OperatingSystem.IsLinux() ? ExecutingPlatform.Linux : ExecutingPlatform.Unknown
        };

        Service.Create<Logger>();
        Service.Create<VelopackUpdateService>();

        // Setup logging
        bool fileLogging = ApplicationSettings.QuerySettings(x => x.UseFileLogging);
        startup.AppVersion = ApplicationSettings.QuerySettings(x => x.ApplicationVersion);
        Service.Get<Logger>().LogMethods = fileLogging ? [LogMethod.File, LogMethod.Notification] : [LogMethod.Notification];

        startup.StartupSteps =
        [
            startup.CheckForUpdatesAsync,
            startup.GetStarCraftLocationAsync,
            startup.VerifyStarCraftLocationAsync,
            startup.VerifyDirectoriesAsync,
            startup.UnzipZipsAsync,
            startup.HandleDependenciesAsync,
            startup.GetModListsAsync,
            startup.GetActiveModsAsync
        ];

        return startup;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Starts the workflow at its first step.
    /// </summary>
    public async Task BeginStartupAsync()
    {
        _startupStep = 0;
        await StartupSteps[_startupStep]?.Invoke();
    }

    /// <summary>
    /// Advances the workflow to its next step or raises <see cref="StartupCompleted"/>.
    /// </summary>
    public async Task ContinueStartupAsync()
    {
        _startupStep++;
        if(_startupStep < StartupSteps.Count)
        {
            await StartupSteps[_startupStep]?.Invoke();
        }
        else
        {
            StartupCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
    #endregion

    #region Private Methods
    private async Task CheckForUpdatesAsync()
    {
        UpdateResult? updateResult = await Service.Get<VelopackUpdateService>().CheckForUpdatesAsync();
        bool available = updateResult != null;
            
        CheckForUpdatesCompleted?.Invoke(this, new(available, updateResult));
    }

    private async Task GetStarCraftLocationAsync()
    {
        // The path is not already saved in our settings
        if(string.IsNullOrWhiteSpace(ApplicationSettings.QuerySettings(x => x.Sc2Path)))
        {
            string sc2SetPath = string.Empty;
            try
            {
                // Thanks to xpaperclip for this block.  Assumes directory based on what is pulled up with .SC2Save files.
                // Only applicable on Windows
                // Disable warning CA1416 "This call site is reachable on all platforms". 
                // This warning can be safely ignored as the platform is safely passed down through the startup creation layer, instead of using pre-processing defines
                #pragma warning disable CA1416
                if(Platform == ExecutingPlatform.Windows)
                {
                    string defaultPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                        "StarCraft II",
                        "StarCraft II.exe");
                    if(FileSystem.FileExists(defaultPath))
                    {
                        sc2SetPath = defaultPath;
                    }

                    using Microsoft.Win32.RegistryKey? key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"Software\Classes\Blizzard.SC2Save\shell\open\command");
                    if(key != null)
                    {
                        object? value = key.GetValue(null);
                        if(value != null)
                        {
                            string s = value.ToString()
                                .Replace(" \"%1\"", "").Trim('\"');
                            s = Path.GetDirectoryName(s); // trim off SC2Switcher.exe
                            s = Path.GetDirectoryName(s); // trim off Support
                            sc2SetPath = Path.Combine(s, @"StarCraft II.exe");
                        }
                    }
                }
                #pragma warning restore CA1416
                else if(Platform == ExecutingPlatform.Linux)
                {
                    // Check the default path for steam compatdata:
                    // ~/.steam/steam/steamapps/compatdata/<id>/pfx/drive_c/Program Files (x86)/StarCraft II
                    string homeFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    string basePath = Path.Combine(homeFolder, ".steam", "steam", "steamapps", "compatdata");

                    // Since StarCraft II is a non-steam game, it will not have a consistent compatdata ID. Trying to search through them to find it.
                    // Since this is a simple file exists check, this shouldn't take too long, but keep an eye on this function for long first-time startup problems.
                    if(FileSystem.DirectoryExists(basePath))
                    {
                        string[] compatData = FileSystem.GetDirectories(basePath);
                        foreach(string dir in compatData)
                        {
                            string testPath = Path.Combine(dir, "pfx", "drive_c", "Program Files (x86)", "StarCraft II", "StarCraft II.exe");
                            if(FileSystem.FileExists(testPath))
                            {
                                sc2SetPath = testPath;
                                break;
                            }
                        }
                    }
                }

                // I don't know what this is on MacOS, nor do I have a Mac to test this on.
                // If someone would like to test/develop a default path for this, it would be much appreciated.
                // Not sure how big the MacOS StarCraft II community is, but this application is technically compatible.
                // TODO: Default MacOS pathing.
            }
            catch (Exception e)
            {
                await Service.Get<Logger>().LogAsync<IOError>([
                    "An error occurred while searching for \"StarCraft II.exe\"",
                    e.ToString()
                ]);
            }

            // Save a discovered path; unsupported or nonstandard installs can be selected by the user.
            if(!string.IsNullOrWhiteSpace(sc2SetPath))
            {
                ApplicationSettings.QuerySettings(x => x.Sc2Path = sc2SetPath);
                ApplicationSettings.QuerySettings(x => x.Sc2Dir = Path.GetDirectoryName(sc2SetPath));
            }
        }

        await ContinueStartupAsync();
    }
    
    private async Task VerifyStarCraftLocationAsync()
    {
        string sc2Path = ApplicationSettings.QuerySettings(x => x.Sc2Path);
        VerifyStarCraftLocationResult result  = string.IsNullOrWhiteSpace(sc2Path) || !FileSystem.FileExists(sc2Path) ? 
            VerifyStarCraftLocationResult.DoesNotExist : 
            VerifyStarCraftLocationResult.Success;
        
        if(result == VerifyStarCraftLocationResult.Success)
        {
            await Service.Get<Logger>().LogAsync<StartupInfo>([
                $"Found StarCraft II.exe at {sc2Path}"
            ]);
        }

        VerifyStarCraftLocationCompleted?.Invoke(this, result);
    }

    private async Task VerifyDirectoriesAsync()
    {
        string sc2Dir = ApplicationSettings.QuerySettings(x => x.Sc2Dir);
        await CreateDirectoryIfNotExistsAsync(Path.Combine(sc2Dir, "Maps", "Campaign"));
        await CreateDirectoryIfNotExistsAsync(Path.Combine(sc2Dir, "Maps", "CustomCampaigns"));
        await CreateDirectoryIfNotExistsAsync(Path.Combine(sc2Dir, "Maps", "Campaign", "swarm"));
        await CreateDirectoryIfNotExistsAsync(Path.Combine(sc2Dir, "Maps", "Campaign", "swarm", "evolution"));
        await CreateDirectoryIfNotExistsAsync(Path.Combine(sc2Dir, "Maps", "Campaign", "void"));
        await CreateDirectoryIfNotExistsAsync(Path.Combine(sc2Dir, "Maps", "Campaign", "voidprologue"));
        await CreateDirectoryIfNotExistsAsync(Path.Combine(sc2Dir, "Maps", "Campaign", "nova"));
        await CreateDirectoryIfNotExistsAsync(Path.Combine(sc2Dir, "Mods"));

        await ContinueStartupAsync();
    }

    private async Task UnzipZipsAsync()
    {
        string sc2Dir = ApplicationSettings.QuerySettings(x => x.Sc2Dir);
        await ZipService.UnzipZipsAsync(sc2Dir);

        await ContinueStartupAsync();
    }

    private async Task HandleDependenciesAsync()
    {
        string sc2Dir = ApplicationSettings.QuerySettings(x => x.Sc2Dir);
        string modSearchPath = Path.Combine(sc2Dir, "Maps", "CustomCampaigns");
        string modsPath = Path.Combine(sc2Dir, "Mods");
        string[] dependencies =
        [
            .. FileSystem.GetFiles(modSearchPath, "*.SC2Mod", SearchOption.AllDirectories),
            .. FileSystem.GetDirectories(modSearchPath, "*.SC2Mod", SearchOption.AllDirectories),
        ];

        foreach(string path in dependencies)
        {
            string name = Path.GetFileName(path);
            string destination = Path.Combine(modsPath, name);
            bool isDirectory = FileSystem.DirectoryExists(path);

            if(FileSystem.FileExists(destination) || FileSystem.DirectoryExists(destination))
            {
                try
                {
                    if(FileSystem.DirectoryExists(destination))
                    {
                        FileSystem.DeleteDirectory(destination, true);
                    }
                    else
                    {
                        FileSystem.DeleteFile(destination);
                    }
                }
                catch(IOException e)
                {
                    await Service.Get<Logger>().LogAsync<IOError>([
                        $"ERROR: Could not replace {name}",
                        e.ToString()
                    ]);
                }
            }

            try
            {
                if(isDirectory)
                {
                    FileSystem.MoveDirectory(path, destination);
                }
                else
                {
                    FileSystem.MoveFile(path, destination);
                }

                await Service.Get<Logger>().LogAsync<IOInfo>([
                    $"Moved {name} to Dependencies folder."
                ]);
            }
            catch(IOException e)
            {
                await Service.Get<Logger>().LogAsync<IOError>([
                    $"ERROR: Could not move {name}",
                    e.ToString()
                ]);
            }
        }
        
        await ContinueStartupAsync();
    }

    private async Task CreateDirectoryIfNotExistsAsync(string path)
    {
        try
        {
            if(!FileSystem.DirectoryExists(path))
            {
                await Task.Run(() => FileSystem.CreateDirectory(path));
            }
        }
        catch (Exception e)
        {
            await Service.Get<Logger>().LogAsync<IOError>([string.Empty, e.ToString()]);
        }
    }
    
    private async Task GetModListsAsync()
    {
        List<ModList> modLists = await CustomCampaignManager.GetModListsAsync();
        GetModListsCompleted?.Invoke(this, modLists);
    }

    private async Task GetActiveModsAsync()
    {
        List<Mod> activeMods = await CustomCampaignManager.GetActiveModsAsync();
        GetActiveModsCompleted?.Invoke(this, activeMods);
    }
    #endregion
}