using System.Diagnostics;
using System.Windows;
using Zenith.NET;
using Zenith.NET.DirectX12;
using Zenith.NET.Views.WPF;

namespace Sample.WPF;

public partial class App : Application
{
    static App()
    {
        Context = GraphicsContext.CreateDirectX12(true);
        Context.ValidationMessage += static (sender, args) => Debug.WriteLine($"[{args.Source} - {args.Severity}] {args.Message}");

        Renderer.Initialize(Context, ZenithView.Output);
    }

    public static GraphicsContext Context { get; }

    protected override void OnExit(ExitEventArgs e)
    {
        Renderer.Shutdown();

        Context.Dispose();

        base.OnExit(e);
    }
}
