using Avalonia;
using Zenith.NET.Views.Avalonia;

namespace Sample.Avalonia;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Renderer.Initialize(ZenithView.Output);

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

        Renderer.Shutdown();
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
                         .UsePlatformDetect()
                         .WithInterFont()
                         .LogToTrace();
    }
}
