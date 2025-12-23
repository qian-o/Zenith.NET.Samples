using System.Windows;
using Zenith.NET.Views.WPF;

namespace Sample.WPF;

public partial class App : Application
{
    public App()
    {
        Renderer.Initialize(ZenithView.Output);

        Exit += static (_, _) => Renderer.Destroy();
    }
}
