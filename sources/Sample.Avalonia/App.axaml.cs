using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Zenith.NET;
using Zenith.NET.Views.Avalonia;
using Zenith.NET.Vulkan;

namespace Sample.Avalonia;

public class App : Application
{
    static App()
    {
        Context = GraphicsContext.CreateVulkan(true);
        Context.ValidationMessage += static (sender, args) => Debug.WriteLine($"[{args.Source} - {args.Severity}] {args.Message}");

        Renderer.Initialize(Context, ZenithView.Output, false);
    }

    public static GraphicsContext Context { get; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}