using System.Windows;

namespace Sample.WPF;

public partial class App : Application
{
    public App()
    {
        Renderer.Initialize();

        Exit += static (_, _) => Renderer.Destroy();
    }
}
