using System.Diagnostics;
using Microsoft.UI.Xaml;
using Sample.WinUI.Views;
using Zenith.NET;
using Zenith.NET.Views.WinUI;
using Zenith.NET.Vulkan;

namespace Sample.WinUI;

public partial class App : Application
{
    static App()
    {
        Context = GraphicsContext.CreateVulkan(true);
        Context.ValidationMessage += static (sender, args) => Debug.WriteLine($"[{args.Source} - {args.Severity}] {args.Message}");

        Renderer.Initialize(Context, ZenithView.Output);
    }

    public App()
    {
        InitializeComponent();
    }

    public static GraphicsContext Context { get; }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        new Window() { Content = new MainView() }.Activate();
    }
}
