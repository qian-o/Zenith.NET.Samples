using System.Diagnostics;
using Sample.Maui.Helpers;
using Sample.Maui.Views;
using Zenith.NET;
using Zenith.NET.Views.Maui;
#if ANDROID
using Zenith.NET.Vulkan;
#elif IOS || MACCATALYST
using Zenith.NET.Metal;
#else
using Zenith.NET.DirectX12;
#endif

namespace Sample.Maui;

public partial class App : Application
{
    static App()
    {
#if ANDROID
        Context = GraphicsContext.CreateVulkan(true);
#elif IOS || MACCATALYST
        Context = GraphicsContext.CreateMetal(true);
#else
        Context = GraphicsContext.CreateDirectX12(true);
#endif
        Context.ValidationMessage += static (sender, args) => Debug.WriteLine($"[{args.Source} - {args.Severity}] {args.Message}");

        Renderer.Initialize(Context, ZenithView.Output, true, MauiAssetService.GetFiles, MauiAssetService.ReadAllBytes);
    }

    public App()
    {
        InitializeComponent();
    }

    public static GraphicsContext Context { get; }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new(new MainView());
    }
}