using Sample.Uno.Views;
using Zenith.NET.Views.WinUI;

namespace Sample.Uno;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        Renderer.Initialize(ZenithView.Output);
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        new Window() { Content = new MainView() }.Activate();
    }
}
