namespace ModManager.StarCraft.Services.Log.Enum;

/// <summary>
/// Specifies where log entries are written or published.
/// </summary>
public enum LogMethod
{
    /// <summary>
    /// Log to a file.
    /// </summary>
    File,

    /// <summary>
    /// Log to the console.
    /// </summary>
    Console,

    /// <summary>
    /// Log to a notification system via an event.
    /// </summary>
    Notification
}