using Avalonia;
using ModManager.StarCraft.Services.Updater;
using System;
using System.Threading.Tasks;
using Velopack;

namespace ModManager.StarCraft.Application;

/// <summary>
/// Main entrance point for the application
/// </summary>
sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Initialize velopack
        // Initializes and runs the Velopack update service. 
        // This method sets up the necessary components and starts the update process.
        // This method should be called at the start of the application to ensure that 
        // the update service is properly initialized.
        VelopackApp.Build().Run();

        // Build avalonia app
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    /// <summary>
    /// Configures the application with Avalonia specific options. 
    /// I left this unchanged, for the most part, from the Avalonia template.
    /// </summary>
    /// <returns></returns>
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}
