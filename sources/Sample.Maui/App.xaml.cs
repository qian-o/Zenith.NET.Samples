using Sample.Maui.Views;

namespace Sample.Maui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        Renderer.Initialize();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new(new MainView());
    }
}