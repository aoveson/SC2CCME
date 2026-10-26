using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ModManager.StarCraft.Application.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
    }
}