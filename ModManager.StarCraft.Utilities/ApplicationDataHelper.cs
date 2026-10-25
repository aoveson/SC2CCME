namespace ModManager.StarCraft.Utilities;

using Interface;

public static class ApplicationDataHelper
{
    public static IApplicationDataFileSystem FileSystem { get; set; } = new PhysicalApplicationDataFileSystem();

    /// <summary>
    /// Resolves an operating-system special-folder path.
    /// </summary>
    public static Func<Environment.SpecialFolder, string> SpecialFolderPathProvider { get; set; } =
        Environment.GetFolderPath;

    /// <summary>
    /// The name of the folder to store the application data in
    /// </summary>
    public const string CCM_DATA_DIRECTORY_NAME = @"SC2CCME";

    /// <summary>
    /// Does a simple safety check to ensure the 'ApplicationData' special folder exists.
    /// </summary>
    /// <returns>True if the folder exists. False if the folder doesn't exist or an exception is thrown</returns>
    /// <remarks>
    /// This should almost always return true. If it doesn't either the OS didn't create the folder, 
    /// The folder was deleted, or the platform doesn't support this folder type. <br /> <br />
    /// In the event this folder doesn't exist, the upper part of the stack trace will not use this folder
    /// and will default to using <c>&lt;current working directory&gt;/SC2CCM</c>.
    /// </remarks>
    private static bool ApplicationDataFolderExists
    {
        get
        {
            try
            {
                return FileSystem.DirectoryExists(SpecialFolderPathProvider(Environment.SpecialFolder.ApplicationData));
            }
            catch(Exception)
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Gets the 'ApplicationData' folder path based on the environment.
    /// </summary>
    /// <remarks>
    /// On Windows: <br />
    /// <c>C:\Users\&lt;username&gt;\AppData\Roaming</c> <br /> <br />
    /// On macOS: <br />
    /// <c>NSApplicationSupportDirectory</c>, typically: <c>/Users/&lt;username&gt;/Library/Application Support</c> <br /> <br />
    /// On Linux: <br />
    /// Returns the path based on the <c>XDG_CONFIG_HOME</c> environment variable, 
    /// defaulting to: <c>$HOME/.config</c> (or <c>~/.config</c>)
    /// </remarks>
    private static string ApplicationDataPath => ApplicationDataFolderExists ?
        SpecialFolderPathProvider(Environment.SpecialFolder.ApplicationData) : Environment.CurrentDirectory;

    /// <summary>
    /// Gets the path to the CCM Data folder 'SC2CCM', where this application should store all of its data.
    /// </summary>
    public static string CCMDataPath => Path.Combine(ApplicationDataPath, CCM_DATA_DIRECTORY_NAME);

    /// <summary>
    /// Gets the path to the CCM data folder.
    /// Creates the folder in the process, if it does not exist
    /// </summary>
    /// <returns>
    /// The path to the CCM Data folder 'SC2CCM', where this application should store all of its data.
    /// </returns>
    public static string GetCCMDataPathCreateIfNotExists()
    {
        if(!FileSystem.DirectoryExists(CCMDataPath))
        {
            FileSystem.CreateDirectory(CCMDataPath);
        }

        return CCMDataPath;
    }
}