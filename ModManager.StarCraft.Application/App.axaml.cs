using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ModManager.StarCraft.Application.ViewModels;
using ModManager.StarCraft.Application.Views;
using System;
using Avalonia.Controls;

namespace ModManager.StarCraft.Application;

/// <summary>
/// The base application class for the StarCraft 2 Custom Campaign Manager Extended.
/// </summary>
public partial class App : Avalonia.Application
{
    /// <summary>
    /// Provides access to the TopLevel of the visual for the application
    /// </summary>
    public static TopLevel? TopLevel { get; private set; }

    /// <summary>
    /// Initializes the application
    /// </summary>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Called when Avalonia framework initialization is complete
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow()
            {
                DataContext = new MainViewModel()
            };
            TopLevel = TopLevel.GetTopLevel(desktop.MainWindow);
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Shuts down the application
    /// </summary>
    public static void ShutdownApplication()
    {
        if(Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }
}