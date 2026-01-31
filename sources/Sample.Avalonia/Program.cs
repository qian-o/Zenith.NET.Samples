using Avalonia;

namespace Sample.Avalonia;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Renderer.Initialize();

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

        Renderer.Destroy();
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
                         .UsePlatformDetect()
                         .WithInterFont()
                         .LogToTrace();
    }
}
