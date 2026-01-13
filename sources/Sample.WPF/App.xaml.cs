using System.Windows;
using Zenith.NET.Views;

namespace Sample.WPF;

public partial class App : Application
{
    public App()
    {
        Renderer.Initialize(ZenithViewOutputs.WPF);

        Exit += static (_, _) => Renderer.Destroy();
    }
}
