using Microsoft.UI.Xaml;

namespace Sample.WinUI;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    public static Window MainWindow { get; } = new();

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
    }
}
