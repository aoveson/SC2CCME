using ModManager.StarCraft.Services.Log.Interface;
using ModManager.StarCraft.Services.Log.IO;
using ModManager.StarCraft.Services.Log.Enum;
using ModManager.StarCraft.Utilities;
using ModManager.StarCraft.Services.Settings;

namespace ModManager.StarCraft.Services.Log;

/// <summary>
/// Defines the logger class responsible for logging messages with different severity levels and methods.
/// </summary>
/// <param name="logWriter">The writer used for file logging.</param>
/// <param name="logFilePath">The path passed to the writer.</param>
/// <param name="logMethods">The methods used to process each entry.</param>
public class Logger(ILogWriter logWriter, string logFilePath, params LogMethod[] logMethods)
{
    private readonly ILogWriter _logWriter = logWriter;

    /// <summary>
    /// Gets or sets the factory used to resolve the default log file path.
    /// </summary>
    public static Func<string> LogFilePathFactory { get; set; } =
        () => Path.Combine(ApplicationDataHelper.GetCCMDataPathCreateIfNotExists(), @"SC2CCME_Log.xml");

    #region Properties
    /// <summary>
    /// Gets the path of the log file used when file logging is enabled.
    /// </summary>
    public string LogFilePath { get; } = logFilePath;

    /// <summary>
    /// Gets or sets the methods used to process log entries.
    /// </summary>
    public List<LogMethod> LogMethods { get; set; } = [.. logMethods];
    #endregion

    #region Events
    /// <summary>
    /// Event triggered when a log entry is created and the log method is set to Notification. Subscribers can handle the log entry as needed.
    /// </summary>
    public event EventHandler<LogEntry>? LogNotification;
    #endregion

    #region Constructors
    /// <summary>
    /// Default empty constructor.
    /// </summary>
    public Logger() : this([]) {}

    /// <summary>
    /// Initializes a new instance of the Logger class with the specified log methods.
    /// This constructor allows for creating a Logger instance that can log messages using multiple methods.
    /// </summary>
    /// <param name="logMethods">The log methods to use.</param>
    public Logger(params LogMethod[] logMethods) : this(new XmlLogWriter(), LogFilePathFactory(), logMethods) {}

    /// <summary>
    /// Initializes a new instance of the Logger class with a single log method.
    /// This constructor allows for creating a Logger instance with a specific log method, which will be used for logging messages. 
    /// </summary>
    /// <param name="logMethod">The log method to use.</param>
    public Logger(LogMethod logMethod): this([logMethod]) {}
    #endregion

    #region Public Methods
    /// <summary>
    /// Logs a message of the specified type T, which must implement ILogType. 
    /// The log entry is created based on the properties of the log type instance and any additional information provided. 
    /// The log entry is then processed according to the current log method (File, Console, or Notification).
    /// </summary>
    /// <typeparam name="T">The type of the log entry to create.</typeparam>
    /// <param name="additionalInfo">Additional information to include in the log entry.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task LogAsync<T>(string[]? additionalInfo = null) where T : ILogType
    {
        additionalInfo ??= [];

        if (typeof(T).GetConstructor(Type.EmptyTypes)?.Invoke(null) is T logInstance)
        {
            if(logInstance is ILogType logType)
            {
                if(ShouldLog(logType.Severity))
                {
                    LogEntry entry = LogEntry.FromLogType(logType, additionalInfo);
                    foreach (LogMethod method in LogMethods)
                    {
                        await LogWithMethodAsync(entry, method);
                    }
                }
            }
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Logs a log entry using the specified log method. This method handles the actual logging process based on the provided log method 
    /// </summary>
    /// <param name="logEntry">The log entry to log.</param>
    /// <param name="method">The log method to use.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task LogWithMethodAsync(LogEntry logEntry, LogMethod method)
    {
        switch (method)
        {
            case LogMethod.File:
                LogCollection collection = _logWriter.Read(LogFilePath);
                collection.LogEntries.Add(logEntry);
                await _logWriter.WriteAsync(collection, LogFilePath);
                break;

            case LogMethod.Notification:
                LogNotification?.Invoke(this, logEntry);
                break;

            case LogMethod.Console:
            default:
                Console.WriteLine($"[{logEntry.Timestamp}][{logEntry.Severity}]: {logEntry.LogTitle} - {logEntry.LogCode}");
                Console.WriteLine($"\t{logEntry.LogMessage}");
                Console.WriteLine($"\t{logEntry.AdditionalInfo}");
                break;
        }
    }

    private bool ShouldLog(LogSeverity severity)
    {
        LogSeverity logLevel = ApplicationSettings.QuerySettings(x => x.LogLevel);
        return logLevel switch
        {
            LogSeverity.Error => severity is LogSeverity.Error,
            LogSeverity.Warning => severity is LogSeverity.Warning or LogSeverity.Error,
            LogSeverity.Info => severity is LogSeverity.Info or LogSeverity.Warning or LogSeverity.Error,
            _ => true,
        };
    }
    #endregion
}