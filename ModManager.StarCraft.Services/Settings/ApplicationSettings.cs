using System.Xml.Serialization;
using ModManager.StarCraft.Utilities;
using ModManager.StarCraft.Services.Settings.Interface;
using ModManager.StarCraft.Services.Updater.Enum;
using ModManager.StarCraft.Services.Log.Enum;

namespace ModManager.StarCraft.Services.Settings;

/// <summary>
/// Represents the application settings persisted by the settings store.
/// </summary>
[XmlType(TypeName="Settings")]
public class ApplicationSettings
{
    /// <summary>Gets or sets the store used to read and persist settings.</summary>
    [XmlIgnore]
    public static ISettingsStore Store { get; set; } = new XmlSettingsStore();

    /// <summary>Gets or sets the factory used to resolve the settings directory.</summary>
    [XmlIgnore]
    public static Func<string> SettingsDataPathFactory { get; set; } =
        ApplicationDataHelper.GetCCMDataPathCreateIfNotExists;

    #region Fields
    [XmlIgnore]
    private bool _settingsUpdated = false;
    #endregion

    #region Const
    [XmlIgnore]
    public const string SETTINGS_FILE_NAME = "SC2CCME.xml";

    [XmlIgnore]
    /// <summary>Gets the full path of the application settings file.</summary>
    public static string SettingsFilePath =>
        Path.Combine(SettingsDataPathFactory(), SETTINGS_FILE_NAME);
    #endregion

    #region Properties
    /// <summary>Gets or sets whether file logging is enabled.</summary>
    [XmlElement]
    public bool UseFileLogging 
    {
        get => _useFileLogging; 
        set
        {
            if(_useFileLogging != value)
            {
                _useFileLogging = value;
                _settingsUpdated = true;
            }
        }
    }
    private bool _useFileLogging = true;

    /// <summary>Gets or sets the maximum file log size in megabytes.</summary>
    [XmlElement]
    public int FileLoggingMaxMegabytes
    {
        get => _fileLoggingMaxMegabytes;
        set
        {
            if(_fileLoggingMaxMegabytes != value)
            {
                _fileLoggingMaxMegabytes = value;
                _settingsUpdated = true;
            }
        }
    }
    private int _fileLoggingMaxMegabytes = 5;

    /// <summary>Gets or sets the minimum severity written to the log.</summary>
    [XmlElement]
    public LogSeverity LogLevel
    {
        get => _logLevel;
        set
        {
            if(_logLevel != value)
            {
                _logLevel = value;
                _settingsUpdated = true;
            }
        }
    }
    private LogSeverity _logLevel = LogSeverity.Info;

    /// <summary>Gets or sets the reported application version.</summary>
    [XmlElement]
    public string ApplicationVersion
    {
        get => _applicationVersion;
        set
        {
            if(!string.Equals(_applicationVersion, value))
            {
                _applicationVersion = value;
                _settingsUpdated = true;
            }
        }
    }
    private string _applicationVersion = "v0.0.0.0";

    /// <summary>Gets or sets whether pre-release updates are accepted.</summary>
    [XmlElement]
    public bool UsePreReleaseUpdates
    {
        get => _usePreReleaseUpdates;
        set
        {
            if(_usePreReleaseUpdates != value)
            {
                _usePreReleaseUpdates = value;
                _settingsUpdated = true;
            }
        }
    }
    private bool _usePreReleaseUpdates = false;

    /// <summary>Gets or sets the GitHub repository used for updates.</summary>
    [XmlElement]
    public string GithubUpdateRepo
    {
        get => _githubUpdateRepo;
        set
        {
            if(!string.Equals(_githubUpdateRepo, value))
            {
                _githubUpdateRepo = value;
                _settingsUpdated = true;
            }
        }
    }
    private string _githubUpdateRepo = "https://github.com/aoveson/SC2CCME";

    /// <summary>Gets or sets the configured update source.</summary>
    [XmlElement]
    public UpdateSourceOption UpdateSource
    {
        get => _updateSource;
        set
        {
            if(_updateSource != value)
            {
                _updateSource = value;
                _settingsUpdated = true;
            }
        }
    }
    private UpdateSourceOption _updateSource = UpdateSourceOption.Github;

    /// <summary>Gets or sets whether an update is waiting to be applied.</summary>
    [XmlElement]
    public bool PendingUpdate
    {
        get => _pendingUpdate;
        set
        {
            if(_pendingUpdate != value)
            {
                _pendingUpdate = value;
                _settingsUpdated = true;
            }
        }
    }
    private bool _pendingUpdate;

    /// <summary>Gets or sets the web endpoint used for updates.</summary>
    [XmlElement]
    public string WebUpdateSourceUri
    {
        get => _webUpdateSourceUri;
        set
        {
            if(_webUpdateSourceUri != value)
            {
                _webUpdateSourceUri = value;
                _settingsUpdated = true;
            }
        }
    }
    private string _webUpdateSourceUri;

    /// <summary>Gets or sets the local directory used for updates.</summary>
    [XmlElement]
    public string DirectoryUpdateSourcePath
    {
        get => _directoryUpdateSourcePath;
        set
        {
            if(_directoryUpdateSourcePath != value)
            {
                _directoryUpdateSourcePath = value;
                _settingsUpdated = true;
            }
        }
    }
    private string _directoryUpdateSourcePath =  Path.Combine(
        ApplicationDataHelper.GetCCMDataPathCreateIfNotExists(),
        "UpdatesPackages"
    );

    /// <summary>Gets or sets whether version downgrades are allowed.</summary>
    [XmlElement]
    public bool AllowDowngrade
    {
        get => _allowDowngrade;
        set
        {
            if(_allowDowngrade != value)
            {
                _allowDowngrade = value;
                _settingsUpdated = true;
            }
        }
    }
    private bool _allowDowngrade = false;

    /// <summary>Gets or sets the target version used for a downgrade.</summary>
    [XmlElement]
    public string DowngradeVersionTarget
    {
        get => _downgradeVersionTarget;
        set
        {
            if(_downgradeVersionTarget != value)
            {
                _downgradeVersionTarget = value;
                _settingsUpdated = true;
            }
        }
    }
    private string _downgradeVersionTarget = string.Empty;

    /// <summary>Gets or sets the path to the StarCraft II executable.</summary>
    [XmlElement]
    public string Sc2Path
    {
        get => _sc2Path;
        set
        {
            if(_sc2Path != value)
            {
                _sc2Path = value;
                _settingsUpdated = true;
            }
        }
    }
    private string _sc2Path = string.Empty;

    /// <summary>Gets or sets the StarCraft II installation directory.</summary>
    [XmlElement]
    public string Sc2Dir
    {
        get => _sc2Dir;
        set
        {
            if(_sc2Dir != value)
            {
                _sc2Dir = value;
                _settingsUpdated = true;
            }
        }
    }
    private string _sc2Dir = string.Empty;
    #endregion

    #region Private Static Methods
    private static ApplicationSettings ReadSettings()
    {
        return Store.Read();
    }

    private static void WriteSettings(ApplicationSettings settings)
    {
        Store.Write(settings);
    }
    #endregion

    #region Public Static Methods
    /// <summary>
    /// Reads settings, applies an action, and persists changes made by the action.
    /// </summary>
    /// <param name="action">The action to apply to the current settings.</param>
    public static void QuerySettings(Action<ApplicationSettings> action)
    {
        ApplicationSettings settings = ReadSettings();
        if(action != null && settings != null)
        {
            action.Invoke(settings);
            if(settings._settingsUpdated)
            {
                WriteSettings(settings);
                settings._settingsUpdated = false;
            }
        }
    }
    
    /// <summary>
    /// Reads settings and returns a selected value, persisting any changes made by the selector.
    /// </summary>
    /// <typeparam name="T">The selected value type.</typeparam>
    /// <param name="func">The selector to apply to the current settings.</param>
    /// <returns>The selected value, or the default value when settings or the selector is null.</returns>
    public static T QuerySettings<T>(Func<ApplicationSettings, T> func)
    {
        T retVal = default;
        ApplicationSettings settings = ReadSettings();
        if(func != null && settings != null)
        {
            retVal = func.Invoke(settings);
            if(settings._settingsUpdated)
            {
                WriteSettings(settings);
                settings._settingsUpdated = false;
            }
        }

        return retVal;
    }
    #endregion

}