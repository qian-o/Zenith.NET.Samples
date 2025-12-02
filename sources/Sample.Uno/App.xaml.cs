using Sample.Uno.Views;
using Zenith.NET;
using Zenith.NET.Views.WinUI;
using Zenith.NET.Vulkan;

namespace Sample.Uno;

public partial class App : Application
{
    static App()
    {
        Context = GraphicsContext.CreateVulkan(true);
        Context.ValidationMessage += static (sender, args) => Console.WriteLine($"[{args.Source} - {args.Severity}] {args.Message}");

        Renderer.Initialize(Context, ZenithView.Output, false);
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
