using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Sample.WinUI.Views;
using Zenith.NET.Views.WinUI;

namespace Sample.WinUI;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        Renderer.Initialize(ZenithView.Output);

        DispatcherQueue.GetForCurrentThread().ShutdownCompleted += static (_, _) => Renderer.Destroy();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        new Window() { Content = new MainView() }.Activate();
    }
}
