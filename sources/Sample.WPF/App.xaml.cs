using System.Windows;
using Zenith.NET;
using Zenith.NET.Views.WPF;
using Zenith.NET.Vulkan;

namespace Sample.WPF;

public partial class App : Application
{
    static App()
    {
        Context = GraphicsContext.CreateVulkan(true);
        Context.ValidationMessage += static (sender, args) => Console.WriteLine($"[{args.Source} - {args.Severity}] {args.Message}");

        Renderer.Initialize(Context, ZenithView.Output);
    }

    public static GraphicsContext Context { get; }
}

