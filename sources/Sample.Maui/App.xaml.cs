using Sample.Maui.Helpers;
using Sample.Maui.Views;
using Zenith.NET.Views.Maui;

namespace Sample.Maui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        Renderer.Initialize(ZenithView.Output, true, MauiAssetService.GetFiles, MauiAssetService.ReadAllBytes);
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new(new MainView());
    }
}