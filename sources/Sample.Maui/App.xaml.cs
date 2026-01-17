using Sample.Maui.Views;
using Zenith.NET.Views.Maui;

namespace Sample.Maui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        Renderer.Initialize(ZenithView.Output);
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new(new MainView());
    }
}