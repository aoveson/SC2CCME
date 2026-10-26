using ModManager.StarCraft.Services.Log;
using ModManager.StarCraft.Services.Log.Enum;
using ModManager.StarCraft.Services.Log.Interface;
using ModManager.StarCraft.Services.Log.IO;
using ModManager.StarCraft.Services.Settings;
using ModManager.StarCraft.Services.Settings.Interface;
using ModManager.StarCraft.Utilities;
using ModManager.StarCraft.Utilities.Interface;

namespace ModManager.StarCraft.Services.UnitTest;

/// <summary>Verifies logger routing, severity filtering, and constructor behavior.</summary>
public sealed class LoggerUnitTest : IDisposable
{
    private readonly Func<string> _originalLogFilePathFactory = Logger.LogFilePathFactory;
    private readonly ISettingsStore _originalSettingsStore = ApplicationSettings.Store;
    private readonly IFileSystem _originalApplicationDataFileSystem = ApplicationDataHelper.FileSystem;

    /// <summary>Configures isolated log-path, settings, and application-data seams.</summary>
    public LoggerUnitTest()
    {
        Logger.LogFilePathFactory = () => "memory";
        ApplicationSettings.Store = new InMemorySettingsStore();
        ModManager.StarCraft.Utilities.ApplicationDataHelper.FileSystem = new InMemoryFileSystem();
    }

    /// <summary>Restores all process-wide seams changed by the fixture.</summary>
    public void Dispose()
    {
        Logger.LogFilePathFactory = _originalLogFilePathFactory;
        ApplicationSettings.Store = _originalSettingsStore;
        ModManager.StarCraft.Utilities.ApplicationDataHelper.FileSystem = _originalApplicationDataFileSystem;
    }

    /// <summary>Publishes a created entry through the notification event.</summary>
    [Fact]
    public async Task NotificationLoggingPublishesTheCreatedEntry()
    {
        Logger logger = new(new InMemoryLogWriter(), "memory", LogMethod.Notification);
        LogEntry? received = null;
        logger.LogNotification += (_, entry) => received = entry;

        await logger.LogAsync<TestLogType>(["first", "second"]);

        Assert.NotNull(received);
        Assert.Equal("Test", received.LogTitle);
        Assert.Equal("Header: first, UNNAMED2: second", received.AdditionalInfo);
    }

    /// <summary>Allows notification logging when no event subscriber is attached.</summary>
    [Fact]
    public async Task NotificationLoggingWithoutSubscribersIsSafe()
    {
        Logger logger = new(new InMemoryLogWriter(), "memory", LogMethod.Notification);

        await logger.LogAsync<TestLogType>();
    }

    /// <summary>Writes file log entries through the injected writer.</summary>
    [Fact]
    public async Task FileLoggingUsesTheInjectedWriter()
    {
        InMemoryLogWriter writer = new();
        Logger logger = new(writer, "memory", LogMethod.File);

        await logger.LogAsync<TestLogType>(["info"]);

        Assert.Single(writer.Written.LogEntries);
        Assert.Equal("Header: info", writer.Written.LogEntries[0].AdditionalInfo);
        Assert.Equal("memory", writer.WrittenPath);
    }

    /// <summary>Routes console and unknown methods to console output.</summary>
    [Fact]
    public async Task ConsoleAndUnknownMethodsWriteToTheConsole()
    {
        StringWriter output = new();
        TextWriter original = Console.Out;
        Console.SetOut(output);

        try
        {
            Logger logger = new(new InMemoryLogWriter(), "memory", LogMethod.Console, (LogMethod)999);
            await logger.LogAsync<TestLogType>();
        }
        finally
        {
            Console.SetOut(original);
        }

        Assert.Contains("Test", output.ToString());
        Assert.Contains("Message", output.ToString());
    }

    /// <summary>Starts a logger with no configured methods by default.</summary>
    [Fact]
    public void DefaultLoggerStartsWithNoLogMethods()
    {
        Logger logger = new(new InMemoryLogWriter(), "memory");

        Assert.Empty(logger.LogMethods);
    }

    /// <summary>Uses the configured log-path factory in all public constructors.</summary>
    [Fact]
    public void PublicConstructorsUseTheConfiguredLogPath()
    {
        Assert.Equal("memory", new Logger().LogFilePath);
        Assert.Equal("memory", new Logger(LogMethod.Console).LogFilePath);
        Assert.Equal("memory", new Logger(LogMethod.Console, LogMethod.Notification).LogFilePath);
    }

    /// <summary>Ignores log types that cannot be constructed without parameters.</summary>
    [Fact]
    public async Task LoggingTypeWithoutParameterlessConstructorIsIgnored()
    {
        Logger logger = new(new InMemoryLogWriter(), "memory", LogMethod.Console);

        await logger.LogAsync<NoDefaultLogType>();
    }

    /// <summary>Applies the configured severity threshold before publishing an entry.</summary>
    [Theory]
    [InlineData(LogSeverity.Error, LogSeverity.Info, false)]
    [InlineData(LogSeverity.Error, LogSeverity.Error, true)]
    [InlineData(LogSeverity.Warning, LogSeverity.Info, false)]
    [InlineData(LogSeverity.Warning, LogSeverity.Warning, true)]
    [InlineData(LogSeverity.Warning, LogSeverity.Error, true)]
    [InlineData(LogSeverity.Info, LogSeverity.Info, true)]
    [InlineData(LogSeverity.Info, LogSeverity.Warning, true)]
    [InlineData(LogSeverity.Info, LogSeverity.Error, true)]
    [InlineData(LogSeverity.Info, LogSeverity.Debug, false)]
    [InlineData(LogSeverity.Debug, LogSeverity.Debug, true)]
    public async Task LoggingHonorsConfiguredSeverity(LogSeverity configuredLevel, LogSeverity messageLevel, bool expectedToLog)
    {
        ApplicationSettings.Store = new InMemorySettingsStore
        {
            Current = new ApplicationSettings { LogLevel = configuredLevel }
        };
        Logger logger = new(new InMemoryLogWriter(), "memory", LogMethod.Notification);
        LogEntry? received = null;
        logger.LogNotification += (_, entry) => received = entry;
        SeverityLogType.CurrentSeverity = messageLevel;

        await logger.LogAsync<SeverityLogType>();

        Assert.Equal(expectedToLog, received != null);
    }

    private sealed class InMemoryLogWriter : ILogWriter
    {
        public LogCollection Written { get; private set; } = new();

        public string WrittenPath { get; private set; } = string.Empty;

        public LogCollection Read(string path) => new();

        public Task WriteAsync(LogCollection collection, string path)
        {
            Written = collection;
            WrittenPath = path;
            return Task.CompletedTask;
        }
    }

    private sealed class TestLogType : ILogType
    {
        public string LogTitle => "Test";

        public LogSeverity Severity => LogSeverity.Info;

        public string LogMessage => "Message";

        public string[] AdditionalInfoHeaders => ["Header"];

        public int LogCode => 42;
    }

    private sealed class NoDefaultLogType : ILogType
    {
        public NoDefaultLogType(string value)
        {
        }

        public string LogTitle => "Unused";

        public LogSeverity Severity => LogSeverity.Info;

        public string LogMessage => "Unused";

        public string[] AdditionalInfoHeaders => [];

        public int LogCode => 0;
    }

    private sealed class SeverityLogType : ILogType
    {
        public static LogSeverity CurrentSeverity { get; set; }

        public string LogTitle => "Severity";

        public LogSeverity Severity => CurrentSeverity;

        public string LogMessage => "Severity message";

        public string[] AdditionalInfoHeaders => [];

        public int LogCode => 43;

    }

    private sealed class InMemorySettingsStore : ISettingsStore
    {
        public ApplicationSettings Current { get; set; } = new();

        public ApplicationSettings Read() => Current;

        public void Write(ApplicationSettings settings) => Current = settings;
    }
}
