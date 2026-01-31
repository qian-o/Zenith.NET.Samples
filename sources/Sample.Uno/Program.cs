using Microsoft.Extensions.Logging;
using Uno.Extensions;
using Uno.UI.Adapter.Microsoft.Extensions.Logging;
using Uno.UI.Hosting;

namespace Sample.Uno;

internal static class Program
{
    [STAThread]
    public static void Main()
    {
        Renderer.Initialize();

        LogExtensionPoint.AmbientLoggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();

            builder.SetMinimumLevel(LogLevel.Information);
        });

        LoggingAdapter.Initialize();

        UnoPlatformHost host = UnoPlatformHostBuilder.Create()
                                                     .App(() => new App())
                                                     .UseX11()
                                                     .UseMacOS()
                                                     .UseWin32()
                                                     .Build();

        host.Run();

        Renderer.Destroy();
    }
}